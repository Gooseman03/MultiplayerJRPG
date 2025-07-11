using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PushableObject : NetworkBehaviour , IQueueUser<NetworkedVector2>
{
    [SerializeField] private LayerMask collisionMask; // What layers the player can collide with
    [SerializeField] private float skinWidth = 0.1f;  // A Extra Buffer when doing collision checks

    private GenericClientReconcile<NetworkedVector2> reconcile;

    private int clientLeadTick = 5;
    uint LastTickPushedAt = 0;
    private float maxPushSpeed = 1;



    // Methods
    private void Awake()
    {
        reconcile = new() { Creator = this };
    }
    public void OnPush(Vector3 angle)
    {
        uint tickBeingRan = (uint)NetworkManager.LocalTime.Tick + (uint)clientLeadTick;
        if (!IsServer)
        {
            LastTickPushedAt = tickBeingRan;
            reconcile.RecordGameState(tickBeingRan, (Vector2)transform.position);
        }
        Vector2 desiredMove = angle * maxPushSpeed;
        if (desiredMove.x != 0)
        {
            Vector2 xMove = new Vector2(desiredMove.x, 0);
            if (!IsColliding(xMove))
            {
                Move(xMove);
            }
            else
            {
                float safeMove = GetSafeMoveDistance(xMove);
                Move(new((safeMove * xMove.normalized).x, 0));
            }
        }
        if (desiredMove.y != 0)
        {
            Vector2 yMove = new Vector2(0, desiredMove.y);
            if (!IsColliding(yMove))
            {
                Move(yMove);
            }
            else
            {
                float safeMove = GetSafeMoveDistance(yMove);
                Move(new(0, (safeMove * yMove.normalized).y));
            }
        }
    }

    // Collision Methods
    private float GetSafeMoveDistance(Vector2 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.localScale, 0f, move.normalized, move.magnitude + skinWidth, collisionMask);
        if (hit.collider == null) return move.magnitude;

        float distanceToHit = hit.distance - skinWidth;
        return Mathf.Max(distanceToHit, 0f);
    }
    private bool IsColliding(Vector2 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.localScale, 0f, move.normalized, move.magnitude + skinWidth, collisionMask);
        return hit.collider != null;
    }

    // OnTick Methods
    public override void OnNetworkSpawn()
    {
        NetworkManager.NetworkTickSystem.Tick += OnTick;
    }
    private void OnTick()
    {
        if (IsServer)
        {
            UpdatePositionRPC((uint)NetworkManager.LocalTime.Tick , transform.position);
            reconcile.queue.Remove((uint)NetworkManager.ServerTime.Tick - 30);
        }
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.NetworkTickSystem.Tick -= OnTick;
    }

    // Generic reconcile Methods
    public bool CheckForDesync(NetworkedVector2 message1, NetworkedVector2 message2)
    {
        return Vector2.Distance(message1, message2) > 0.1f;
    }
    public NetworkedVector2 Resync(NetworkedVector2 predicted, NetworkedVector2 server)
    {
        NetworkedVector2 offset = server - predicted;
        transform.position += (Vector3)offset.Value;
        return offset;
    }
    public void OnMessageNotStored(uint messageId, NetworkedVector2 message)
    {
        if (LastTickPushedAt < messageId)
        {
            transform.position = message;
        }
    }
    public void ApplyOffset(uint key, NetworkedVector2 offset)
    {
        if (reconcile.queue.ContainsKey(key))
        {
            reconcile.queue[key] += offset;
        }
    }

    // RPCs
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    public void UpdatePositionRPC(uint messageId, NetworkedVector2 newposition)
    {
        // Perform client-side prediction correction
        reconcile.IsPredictionCorrect(newposition, messageId);
        return;
    }

    // Helper Methods
    void Move(Vector2 vector2)
    {
        Vector3 NewPosition = new Vector3
            (
            transform.position.x + vector2.x,
            transform.position.y + vector2.y,
            0
            );
        transform.position = NewPosition;
    }
}