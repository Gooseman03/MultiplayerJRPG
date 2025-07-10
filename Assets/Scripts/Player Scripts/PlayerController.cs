using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using Ladder.DebugHelper;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [HideInInspector]
    public Queue<Inputs> Queue = new Queue<Inputs>(); // Stores input messages for server-side processing

    [HideInInspector]
    public PlayerMovement playerMovement = null; // Reference to the movement script

    private ClientReconcile clientReconcile; // Handles client-side prediction correction
    private Interpolation interpolator; // Smooths other clients movement on this client

    [SerializeField] private bool isExtrapolating = false;

    [Header("Buffer System")]
    [Tooltip("Number of past inputs the client sends to the server.\nDefault is 4.")]
    [SerializeField] private int redundantMessages = 4;
    [Tooltip("Maximum number of redundant messages the buffer health system can use.\nDefault is 8.")]
    [SerializeField] private int maxRedundantMessages = 8;
    [Tooltip("Number of ticks the client leads ahead of the server.\nDefault is 4.")]
    [SerializeField] private int clientLeadTick = 3;

    // Custom tick system variables 
    private double TickSpeed = 0;   // Time between ticks
    private double TickTimer = 0;   // Timer tracking tick intervals
    private double TickOffset = 0;  // Adjustment for speeding up/slowing down ticks

    private void Awake()
    {
        clientReconcile = GetComponent<ClientReconcile>();
    }
    public override void OnNetworkSpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();
        NetworkManager.NetworkTickSystem.Tick += OnTick;
        
        if (IsLocalPlayer)
        {
            gameObject.AddComponent<CameraController>();
        }
        if (!IsLocalPlayer)
        {
            if (IsServer)
            {
                interpolator = gameObject.AddComponent<HostInterpolation>();
                ((HostInterpolation)interpolator).EnableExtrapolation = isExtrapolating;
            }
            else
            {
                interpolator = gameObject.AddComponent<Interpolation>();
            }
            //interpolator.Queue = Queue;
        }
        //DebugMultiplayerUi.Instance.ExtrapolationToggle.onValueChanged.AddListener((value) => { Extrapolate = value; });
        //DebugMultiplayerUi.Instance.InterpolationToggle.onValueChanged.AddListener((value) => { playerMovement.Interpolate = value; });
        //DebugMultiplayerUi.Instance.AheadTicksSlider.onValueChanged.AddListener((value) => { clientLeadTick = (int)value; });
        //DebugMultiplayerUi.Instance.RedundantTicksSlider.onValueChanged.AddListener((value) => { redundantMessages = (int)value; });
    }
    public override void OnNetworkDespawn()
    {
        // Unsubscribe from tick updates when despawned
        NetworkManager.NetworkTickSystem.Tick -= OnTick;
    }

    // Sends the current and recent inputs to the server for prediction and reconciliation
    private void SendInputsMessage(Vector2 movementVector, bool isAttacking)
    {
        // Create an input bundle from the current state
        Inputs bundle = new Inputs(movementVector, isAttacking);

        // Calculate the tick that will be processed next
        uint tickBeingRan = (uint)NetworkManager.LocalTime.Tick + (uint)clientLeadTick;

        // If this tick has already been processed locally, just resend redundant input history
        if (clientReconcile.CheckForInputAt(tickBeingRan))
        {
            // Get recent redundant messages to send to the server
            StoredMessage<Inputs>[] messagesToSend = clientReconcile.GrabRedundantMessages(tickBeingRan, redundantMessages);

            // Send them to the server (reliable delivery isn't required)
            InputReportRPC(messagesToSend);
            return;
        }

        // Record the current input so it can be used later for reconciliation
        clientReconcile.RecordGameState(tickBeingRan, bundle);

        // Get the full input history, including the current input (which was just recorded)
        StoredMessage<Inputs> [] messages = clientReconcile.GrabRedundantMessages(tickBeingRan, redundantMessages + 1);

        // Send the inputs to the server
        InputReportRPC(messages);
    }

    private void Update()
    {
        TickSpeed = NetworkManager.ServerTime.FixedDeltaTime;
        
        // Only run input logic only for the local player
        if (IsLocalPlayer)
        {
            TickTimer += Time.deltaTime;

            if (TickTimer >= TickSpeed - TickOffset)
            {
                OnCustomTick();
            }
        }
    }
    // Called in the update when the custom ticks trigger
    private void OnCustomTick()
    {
        Vector2 MovementVector = InputManager.Move.normalized;
        bool IsAttacking = InputManager.Attack;

        if (MovementVector != Vector2.zero)
        {
            // Apply movement locally
            playerMovement.MovePlayerWithCollisions(MovementVector);

            // Send inputs to server (if not host)
            if (!IsServer)
            {
                SendInputsMessage(MovementVector, IsAttacking);
            }
        }
        TickTimer = 0; // Reset tick timer
    }

    // Dynamically adjusts tick offset and redundancy based on buffer health
    private void AdjustTickOffsetBasedOnBuffer(int actual, int target)
    {
        // Set a tolerance so we don't constantly change the tickrate
        int tolerance = 1;

        // If the buffer is too small, increase the tick rate and redundantMessages to account for possible packetloss
        if (actual < target - tolerance)
        {
            TickOffset = NetworkManager.NetworkTickSystem.LocalTime.TickOffset / 30;
            if (redundantMessages < maxRedundantMessages)
            {
                redundantMessages++;
            }
        }
        // If the buffer is too large, decrease the tick rate and reduce redundantMessages (but not below 0) so we don't hog bandwidth
        else if (actual > target + tolerance)
        {
            TickOffset = -NetworkManager.NetworkTickSystem.LocalTime.TickOffset / 30;
            redundantMessages = Mathf.Max(0, redundantMessages - 1);
        }
        else
        // If we’re on time, make no changes
        {
            TickOffset = 0;
        }
    }

    // Server-side tick processing
    private void OnTick()
    {
        if (IsServer)
        {
            // If this is the host player, just send current position to clients and return
            if (IsLocalPlayer)
            {
                UpdatePositionRPC((uint)NetworkManager.LocalTime.Tick,new (transform.position, InputManager.Attack));
                return;
            }

            // Set a default inputs incase the client didnt send a message this assumes no input from the player
            Inputs nextInputs = new();
            // Try to get input for the current tick from the message queue
            // If its found set that as the next inputs to run and delete all inputs before then
            if (Queue.TryGetValue((uint)NetworkManager.LocalTime.Tick, out Inputs inputs))
            {
                nextInputs = inputs;
                Queue.Remove((uint)NetworkManager.LocalTime.Tick - 30);
            }

            // Apply movement on server side
            playerMovement.MovePlayerWithCollisions(nextInputs.MoveInput.normalized);

            Inputs newPosition = new(transform.position, inputs.IsAttacking);

            // Do interpolation for the host version of the clients
            // The HostInterpolation creates a visual object that has interpolation and follows the real position
            interpolator.AddNewPosition((uint)NetworkManager.LocalTime.Tick, newPosition.Position);

            // Send position to the clients 
            UpdatePositionRPC((uint)NetworkManager.LocalTime.Tick, newPosition);
        }
    }

    // RPC from client to server: reports input messages
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void InputReportRPC(StoredMessage<Inputs>[] messages)
    {
        // Set the size for ServerSideBuffer that the client should aim to keep
        int targetBuffer = 4;

        // For each Input the Client sent add it to the buffer if we havent received it before
        for (int i = 0; i < messages.Length; i++)
        {
            Queue.TryAddValue((uint)NetworkManager.ServerTime.Tick, messages[i]);
        }

        // Calculate how large the buffer of future messages is
        int actualBuffer = (int)(messages[^1].Id - (uint)NetworkManager.ServerTime.Tick);

        // Send a message to the client with the health of the buffer
        SendBufferStatusClientRPC(actualBuffer, targetBuffer);
    }

    // RPC from server to client: tells client how many ticks ahead/behind it is aka the current buffer health
    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
    private void SendBufferStatusClientRPC(int actualBuffer, int targetBuffer)
    {
        AdjustTickOffsetBasedOnBuffer(actualBuffer, targetBuffer);
    }

    // RPC from server to all clients: updates the position of a player
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void UpdatePositionRPC(uint messageId, Inputs newInputs)
    {
        if (IsLocalPlayer)
        {
            // Perform client-side prediction correction
            clientReconcile.IsPredictionCorrect(newInputs, messageId);
            return;
        }
        else
        {
            interpolator.AddNewPosition(messageId, newInputs.Position);
            return;
        }
    }
}
