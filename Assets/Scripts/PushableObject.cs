using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PushableObject : NetworkBehaviour , IQueueUser<NetworkedVector2>
{
    private GenericClientReconcile<NetworkedVector2> reconcile;
    //public List<uint> Keys;
    //public List<NetworkedVector2> Values;
    private int clientLeadTick = 5;
    uint LastTickPushedAt = 0;
    private float maxPushSpeed = 1;

    //private Vector3 Lastposition;
    private void Awake()
    {
        reconcile = new() { Creator = this };
    }

    [SerializeField] private LayerMask collisionMask; // What layers the player can collide with
    [SerializeField] private float skinWidth = 0.1f;  // A Extra Buffer when doing collision checks
    // Methods
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
        if (hit && hit.transform.TryGetComponent(out PushableObject pushableObject))
        {
            pushableObject.OnPush(move);
        }
        return hit.collider != null;
    }
    public void OnPush(Vector3 angle)
    {
        // This must be ran pre move
        uint tickBeingRan = (uint)NetworkManager.LocalTime.Tick + (uint)clientLeadTick;
        if (IsClient)
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
    public override void OnNetworkSpawn()
    {
        NetworkManager.NetworkTickSystem.Tick += OnTick;
    }
    private void OnTick()
    {
        if (IsServer)
        {
            UpdatePositionRPC((uint)NetworkManager.ServerTime.Tick, transform.position);
            //Keys = reconcile.queue.Keys.ToList();
            //Values = reconcile.queue.Values;
            //if (reconcile.queue.TryGetValue((uint)NetworkManager.LocalTime.Tick, out NetworkedVector2 vectors))
            //{
            //    
            //}
            reconcile.queue.Remove((uint)NetworkManager.ServerTime.Tick - 30);
        }
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void UpdatePositionRPC(uint messageId, NetworkedVector2 newposition)
    {
        // Perform client-side prediction correction
        reconcile.IsPredictionCorrect(newposition, messageId);
        return;
    }

    public bool CheckForDesync(NetworkedVector2 message1, NetworkedVector2 message2)
    {
        NetworkedVector2 difference = message1 - message2;
        if (Mathf.Abs(difference.x) > 0.1f || Mathf.Abs(difference.y) > 0.1f)
        {
            return true;
        }
        return false;
    }

    public NetworkedVector2 Resync(uint messageId, NetworkedVector2 message)
    {
        NetworkedVector2 Offset = reconcile.queue[messageId] - message;
        Debug.Log("A Object Desynced at MessageID " + messageId + "... Correcting by " + Offset);
        transform.position -= (Vector3)Offset.Value;
        return Offset;
    }

    public void OnMessageNotStored(uint messageId, NetworkedVector2 message)
    {
        if (LastTickPushedAt < messageId)
        {
            transform.position = message;
        }
        //Debug.Log("Someone else moved me on " + messageId + "... Correcting");
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

    public NetworkedVector2 ApplyOffset(uint key, NetworkedVector2 message)
    {
        return reconcile.queue[key] = reconcile.queue[key] - message;
    }
}