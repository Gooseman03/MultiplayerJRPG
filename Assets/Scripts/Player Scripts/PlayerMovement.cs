using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;

    private ClientReconcile clientReconcile = null;
    private ServerQueue serverQueue = null;

    private void Awake()
    {
        clientReconcile = GetComponent<ClientReconcile>();
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.NetworkTickSystem.Tick += OnTick;
        if (IsServer)
        {
            serverQueue = new ServerQueue(this);
        }
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.NetworkTickSystem.Tick -= OnTick;
    }

    private void OnTick()
    {
        if (IsServer)
        {
            int i = 0;
            while (serverQueue.GrabNextMessage(out MessageBundle message))
            {
                Movement(message.Input.normalized);
                UpdatePositionRPC(transform.position, message.Id);
            }
        }
        if (!IsOwner) return;
        Vector2 MovementVector = InputManager.Move.normalized;
        if (MovementVector != Vector2.zero)
        {
            Movement(MovementVector);
            if (!IsServer)
            {
                clientReconcile.SendMovementMessage(MovementVector);
            }
        }
    }

    //Assumes Vector2 will be normalized in the rpc
    public void Movement(Vector2 movementVector)
    {
        // Length of one tick
        float deltaTime = NetworkManager.Singleton.NetworkTickSystem.LocalTime.FixedDeltaTime;
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
    public void UpdatePositionRPC(Vector2 NewPosition, ulong MessageId)
    {
        if(IsLocalPlayer)
        {
            clientReconcile.IsPredictionCorrect(NewPosition, MessageId);
            return;
        }
        transform.position = NewPosition;
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    public void MovementRequestRPC(Vector2 ClientInputVector, ulong MessageId)
    {
        serverQueue.AddMessageToQueue(ClientInputVector.normalized, MessageId);
    }
}
