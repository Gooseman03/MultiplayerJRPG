using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using Ladder.DebugHelper;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public ServerQueue Queue = null; // Queue storing input messages for server-side processing
    public PlayerMovement playerMovement = null; // Reference to the Movement script

    private ClientReconcile clientReconcile = null; // Client-side reconciliation for correcting incorrect predictions
    private ServerInputAssumption extrapolation = null; // Handles extrapolation when inputs are missing

    [SerializeField] private bool Extrapolate = false; // Enable extrapolation for missing client inputs
    [SerializeField] private int redundantMessages = 4; // Number of past input messages to resend to mitigate packet loss
    [SerializeField] private int maxRedundantMessages = 10;
    [SerializeField] private int clientLeadTick = 4; // Tick offset to allow client prediction ahead of the server

    private void Awake()
    {
        clientReconcile = GetComponent<ClientReconcile>();
        Queue = new ServerQueue();
    }
    public override void OnNetworkSpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();
        NetworkManager.NetworkTickSystem.Tick += OnTick;
        
        // If this is the server, initialize extrapolation logic
        if (IsServer)
        {
            extrapolation = GetComponent<ServerInputAssumption>();
        }

        DebugMultiplayerUi.Instance.ExtrapolationToggle.onValueChanged.AddListener((value) => { Extrapolate = value; });
        DebugMultiplayerUi.Instance.InterpolationToggle.onValueChanged.AddListener((value) => { playerMovement.Interpolate = value; });
        DebugMultiplayerUi.Instance.AheadTicksSlider.onValueChanged.AddListener((value) => { clientLeadTick = (int)value; });
        DebugMultiplayerUi.Instance.RedundantTicksSlider.onValueChanged.AddListener((value) => { redundantMessages = (int)value; });
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from tick updates when despawned
        NetworkManager.NetworkTickSystem.Tick -= OnTick;
    }

    // Sends input to the server (along with redundant messages for reliability)
    public void SendInputsMessage(Vector2 movementVector, bool isAttacking)
    {
        Inputs bundle = new Inputs(movementVector, isAttacking);
        uint futureTick = (uint)NetworkManager.LocalTime.Tick + (uint)clientLeadTick;
        
        // Checks if the current tick has been run multiple times
        if (clientReconcile.DoesPreviousInputExistAt(futureTick)) 
        {
            // Grab recent input history to send to server (includes redundant messages)
            MessageBundle[] messagesToSend = clientReconcile.GrabPreviousInputs(futureTick, redundantMessages);

            // Send input messages to the server
            InputReportRPC(messagesToSend);
            return;
        }

        // Record the game state for future reconciliation
        clientReconcile.RecordGameState(futureTick, bundle);

        // Grab recent input history to send to server (includes redundant messages)
        // This Includes the Input that was most recently Executed
        MessageBundle[] messages = clientReconcile.GrabPreviousInputs(futureTick, redundantMessages + 1);

        // Send input messages to the server
        InputReportRPC(messages);
    }

    double TickSpeed = 0; // Interval between ticks
    double TickTimer = 0; // Timer tracking time elapsed between ticks
    double TickOffset = 0; // To speed up or slow down Ticks

    private void Update()
    {
        TickSpeed = NetworkManager.ServerTime.FixedDeltaTime;
        
        // Only run input logic for the local player
        if (IsLocalPlayer)
        {
            TickTimer += Time.deltaTime;

            if (TickTimer >= TickSpeed - TickOffset)
            {
                Vector2 MovementVector = InputManager.Move.normalized;
                bool IsAttacking = InputManager.Attack;

                // Apply Locally Predicted Movement
                if (MovementVector != Vector2.zero)
                {
                    playerMovement.Movement(MovementVector);

                    // Sends Inputs to the server (Client-side only)
                    if (!IsServer)
                    {
                        SendInputsMessage(MovementVector, IsAttacking);
                    }
                }

                TickTimer = 0; // Reset tick timer
            }
        }
    }

    // Calculates the tick offset and trys to keep it near the target will increase and decrease as needed
    private void AdjustTickOffsetBasedOnBuffer(int actual, int target)
    {
        int tolerance = 1;

        if (actual < target - tolerance)
        {
            TickOffset = NetworkManager.NetworkTickSystem.LocalTime.TickOffset / 30;
            if (redundantMessages < maxRedundantMessages)
            {
                redundantMessages++;
            }
        }
        else if (actual > target + tolerance)
        {
            TickOffset = -NetworkManager.NetworkTickSystem.LocalTime.TickOffset / 30;
            redundantMessages = Mathf.Max(0, redundantMessages - 1);
        }
        else
        {
            TickOffset = 0;
        }
    }


    //bool PlayerStopped = false;
    // Called Every Network Tick
    private void OnTick()
    {
        if (IsServer)
        {
            // If this is the host player, just send current position and return
            if (IsLocalPlayer)
            {
                UpdatePositionRPC(transform.position, 0);
                return;
            }

            Inputs nextInputs = new();
            // Try to get input for the current tick from the message queue
            if (Queue.TryGetInputsAt((uint)NetworkManager.ServerTime.Tick, out Inputs inputs))
            {
                nextInputs = inputs;
                Queue.SetGoodMessage((uint)NetworkManager.ServerTime.Tick, nextInputs);
                Queue.RemoveMessageFromBuffer((uint)NetworkManager.ServerTime.Tick);
            }
            else
            {
                // Extrapolate if missing inputs and extrapolation is enabled
                if (Extrapolate && extrapolation != null)
                {
                    if
                        (
                        Queue.LastGoodMessage.MoveInput != Vector2.zero &&
                        Queue.TryGetInputsAt((uint)NetworkManager.ServerTime.Tick + 1, out Inputs futureInputs)
                        )
                    {
                        nextInputs = extrapolation.ExtrapolateMissingInputs(Queue.LastGoodMessage.Inputs, futureInputs);
                    }
                }
            }

            //if (nextInputs.MoveInput != Vector2.zero)
            //{
            //    PlayerStopped = false;
            //} 
            //else if (!PlayerStopped)
            //{
            //    PlayerStopped = true;
            //}
            //else
            //{
            //    return;
            //}

            // Apply movement on server side
            playerMovement.Movement(nextInputs.MoveInput.normalized);

            // Send position back to clients for reconciliation
            UpdatePositionRPC(transform.position, (uint)NetworkManager.ServerTime.Tick);
        }
    }

    // RPC to report inputs to the server
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    public void InputReportRPC(MessageBundle[] messages)
    {
        int targetBuffer = 4;

        
        for (int i = 0; i < messages.Length; i++)
        {
            Queue.TryAddMessageToBuffer((uint)NetworkManager.ServerTime.Tick, messages[i]);
        }
        int actualBuffer = (int)(messages[^1].Id - (uint)NetworkManager.ServerTime.Tick);
        SendBufferStatusClientRPC(actualBuffer, targetBuffer);
    }

    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
    private void SendBufferStatusClientRPC(int actualBuffer, int targetBuffer)
    {
        AdjustTickOffsetBasedOnBuffer(actualBuffer, targetBuffer);
    }

    // RPC to update client-side position, used for reconciliation or direct sync
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    public void UpdatePositionRPC(Vector2 NewPosition, uint MessageId)
    {
        if (IsLocalPlayer)
        {
            // Perform client-side prediction correction
            clientReconcile.IsPredictionCorrect(NewPosition, MessageId);
            return;
        }
        else
        {
            // Apply directly to remote clients
            playerMovement.OnNewPositionRecieved(NewPosition);
        }
    }
}
