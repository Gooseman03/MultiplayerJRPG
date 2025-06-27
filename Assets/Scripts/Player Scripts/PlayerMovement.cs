using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;
    private ClientReconcile clientReconcile = null;
    public ServerQueue serverQueue = null;
    private ClientInterpolation interpolation = null;
    private ServerInputAssumption extrapolation = null;
    [SerializeField] private bool Interpolate = false;
    [SerializeField] private bool Extrapolate = false;

    [SerializeField] private int redundantMessages = 4;
    [SerializeField] private int clientLeadTick = 5;

    private void Awake()
    {
        clientReconcile = GetComponent<ClientReconcile>();
        serverQueue = new ServerQueue(this);
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.NetworkTickSystem.Tick += OnTick;
        if (!IsLocalPlayer && !IsServer)
        {
            interpolation = GetComponent<ClientInterpolation>();
        }
        if (IsServer)
        {
            extrapolation = GetComponent<ServerInputAssumption>();
        }
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.NetworkTickSystem.Tick -= OnTick;
    }

    float TickSpeed = 0;
    float TickTimer = 0;
    private void Update()
    {

        TickSpeed = NetworkManager.ServerTime.FixedDeltaTime;
        if (!IsLocalPlayer)
        {
            if (Interpolate && interpolation != null)
            {
                transform.position = interpolation.FindNextMove();
            }
        }

        if (IsLocalPlayer)
        {
            TickTimer += Time.deltaTime;
            if (TickTimer >= TickSpeed)
            {
                Vector2 MovementVector = InputManager.Move.normalized;
                if (MovementVector != Vector2.zero)
                {
                    Movement(MovementVector);
                }
                if (!IsServer)
                {
                    SendMovementMessage(MovementVector);
                }
                TickTimer = 0;
            }

        }
    }
    public void SendMovementMessage(Vector2 movementVector)
    {
        clientReconcile.StampLocation((uint)NetworkManager.LocalTime.Tick + (uint)clientLeadTick, movementVector);
        Dictionary<uint, Vector2> messages = clientReconcile.GrabPreviousInputs(redundantMessages, (uint)NetworkManager.LocalTime.Tick + (uint)clientLeadTick);
        Vector2[] vector2s = new Vector2[messages.Count];
        uint[] ids = new uint[messages.Count];
        int i = 0;
        foreach (KeyValuePair<uint, Vector2> message in messages)
        {
            vector2s[i] = message.Value;
            ids[i] = message.Key;
            i++;
        }
        MovementRequestRPC(vector2s, ids);
    }
    private void OnTick()
    {
        if (IsServer)
        {
            if (IsLocalPlayer)
            {
                UpdatePositionRPC(transform.position, 0);
                return;
            }
            MessageBundle nextMessage = new() { Id = (uint)NetworkManager.ServerTime.Tick, Input = Vector2.zero };
            if (serverQueue.TryGetMessageAt((uint)NetworkManager.ServerTime.Tick, out MessageBundle message))
            {
                nextMessage = message;
                serverQueue.SetGoodMessage(nextMessage);
                serverQueue.RemoveMessageFromBuffer((uint)NetworkManager.ServerTime.Tick);
            }
            else
            {
                if (Extrapolate && extrapolation != null)
                {
                    if
                        (
                        serverQueue.LastGoodMessage.Input != Vector2.zero &&
                        serverQueue.TryGetMessageAt((uint)NetworkManager.ServerTime.Tick + 1, out MessageBundle futureMessage)
                        )
                    {
                        nextMessage = extrapolation.ExtrapolateMissingMessage
                            (
                            (uint)NetworkManager.ServerTime.Tick,
                            serverQueue.LastGoodMessage, futureMessage
                            );
                    }
                }
            }
            Movement(nextMessage.Input.normalized);
            UpdatePositionRPC(transform.position, nextMessage.Id);
            //redundancy.ApplyRedundancyToClient();
        }
    }

    //Assumes Vector2 will be normalized in the rpc
    public void Movement(Vector2 movementVector)
    {
        // Length of one tick
        float deltaTime = NetworkManager.Singleton.NetworkTickSystem.ServerTime.FixedDeltaTime;
        MovePlayer(movementVector * speed * deltaTime);
    }

    void MovePlayer(Vector2 vector2)
    {
        Vector3 NewPosition = new Vector3
            (
            transform.position.x + vector2.x,
            transform.position.y + vector2.y,
            0
            );
        transform.position = NewPosition;
    }

    [Rpc(SendTo.NotServer)]
    public void UpdatePositionRPC(Vector2 NewPosition, uint MessageId)
    {
        if (IsLocalPlayer)
        {
            clientReconcile.IsPredictionCorrect(NewPosition, MessageId);
            return;
        }
        else
        {
            if (Interpolate && interpolation != null)
            {
                interpolation.SetRecievedPosition(NewPosition);
                return;
            }
            transform.position = NewPosition;
            return;
        }
    }

    //[Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    //public void MovementRequestRPC(Vector2 ClientInputVector, uint MessageId)
    //{
    //    serverQueue.TryAddMessageToBuffer(ClientInputVector.normalized, MessageId);
    //}
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    public void MovementRequestRPC(Vector2[] ClientInputVector, uint[] MessageId)
    {
        for (int i = 0; i < MessageId.Length; i++)
        {
            serverQueue.TryAddMessageToBuffer(ClientInputVector[i].normalized, MessageId[i]);
        }
    }
}
