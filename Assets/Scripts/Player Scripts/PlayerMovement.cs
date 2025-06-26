using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;

    private ClientReconcile clientReconcile = null;
    private ServerQueue serverQueue = null;
    private ClientInterpolation interpolation = null;
    private ServerInputAssumption extrapolation = null;
    [SerializeField] private bool Interpolate = false;
    [SerializeField] private bool Extrapolate = false;

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
        TickSpeed = NetworkManager.Singleton.NetworkTickSystem.ServerTime.FixedDeltaTime;
        if (!IsLocalPlayer) 
        {
            if (Interpolate && interpolation != null)
            {
                transform.position = interpolation.FindNextMove();
            }
        }
        
        if(IsLocalPlayer)
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
                    clientReconcile.SendMovementMessage(MovementVector);
                }
                TickTimer = 0;
            }
            
        }
    }

    private void OnTick()
    {
        if (IsServer)
        {
            if (IsLocalPlayer)
            {
                UpdatePositionRPC(transform.position, 0);
            }
            List<MessageBundle> queue = serverQueue.GetMessageQueue();
            //queue.Sort((mes1, mes2) => { return mes1.Id.CompareTo(mes2.Id); });

            // More Dangerous This will assume the client message was late and Create a new one to fill the gap
            /*
            if (queue.Count == 0)
            {
                Debug.Log("Message Was Late... Extrapolating");
                extrapolation.ExtrapolateWithoutCare();
            }
            else if (Extrapolate && extrapolation != null)
            {
                queue = extrapolation.FindMissingMessage(queue);
            }
            */
            /* 
             * Extrapolation Finds Missing messages and returns a list with Guessed Inputs
             * Will only Insert missing messages if the messages before and after the missing one were the same
            */
            if (extrapolation != null)
            {
                if (Extrapolate)
                {
                    queue = extrapolation.FindMissingMessage(queue);
                }
            }
            foreach (MessageBundle message in queue)
            {
                Movement(message.Input.normalized);
                UpdatePositionRPC(transform.position, message.Id);
            }
            
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
    public void UpdatePositionRPC(Vector2 NewPosition, ulong MessageId)
    {
        if(IsLocalPlayer)
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

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    public void MovementRequestRPC(Vector2 ClientInputVector, ulong MessageId)
    {
        serverQueue.AddMessageToQueue(ClientInputVector.normalized, MessageId);
    }
    public void MovementRequestRPC(Vector2[] ClientInputVector, ulong[] MessageId)
    {
        for (int i = 0; i < MessageId.Length; i++)
        {
            serverQueue.AddMessageToQueue(ClientInputVector[i].normalized, MessageId[i]);
        }
    }
}
