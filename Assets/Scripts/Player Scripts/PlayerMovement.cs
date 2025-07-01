using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;

    private PlayerController controller; // Reference to the PlayerController
    private ClientInterpolation interpolation = null; // Reference to the Interpolator
    public bool Interpolate = false;
    private List<RaycastHit2D> raycastHits = new List<RaycastHit2D>();
    [SerializeField] private LayerMask collisionMask;
    private Vector2 colliderSize;
    [SerializeField] private float skinWidth = 0.1f;
    // Assigns necessary references 
    public override void OnNetworkSpawn()
    {
        controller = GetComponent<PlayerController>();
        if (!IsLocalPlayer && !IsServer)
        {
            interpolation = GetComponent<ClientInterpolation>();
        }
        colliderSize = transform.localScale;
    }

    // Ticks the Interpolater to the next position if enabled
    private void Update()
    {
        if (!IsLocalPlayer)
        {
            if (Interpolate && interpolation != null)
            {
                transform.position = interpolation.FindNextMove();
            }
        }
    }
    // Will Convert the vector2 Supplied into a Movement vector
    // Assumes supplied Vector2 will be normalized in the rpc
    public void Movement(Vector2 movementVector)
    {
        float deltaTime = NetworkManager.Singleton.NetworkTickSystem.ServerTime.FixedDeltaTime;
        
        Vector2 desiredMove = movementVector * speed * deltaTime;
        if (desiredMove.x != 0)
        {
            Vector2 xMove = new Vector2(desiredMove.x, 0);
            if (!IsColliding(xMove))
            {
                MovePlayer(xMove);
            }
            else
            {
                float safeMove = GetSafeMoveDistance(xMove);
                MovePlayer(new((safeMove * xMove.normalized).x, 0));
            }
        }
        if (desiredMove.y != 0)
        {
            Vector2 yMove = new Vector2(0, desiredMove.y);
            if (!IsColliding(yMove))
            {
                MovePlayer(yMove);
            }
            else
            {
                float safeMove = GetSafeMoveDistance(yMove);
                MovePlayer(new(0, (safeMove * yMove.normalized).y));
            }
        }
    }

    private bool IsColliding(Vector2 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, colliderSize, 0f, move.normalized, move.magnitude + skinWidth, collisionMask);
        return hit.collider != null;
    }

    private float GetSafeMoveDistance(Vector2 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, colliderSize, 0f, move.normalized, move.magnitude + skinWidth, collisionMask);
        if (hit.collider == null) return move.magnitude;

        float distanceToHit = hit.distance - skinWidth;
        return Mathf.Max(distanceToHit, 0f);
    }

    // Moves the Transform to the vector2 supplied will assume z is 0 
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
    /* 
     * If the interpolater is enabled it will insert newPosition as the next position for the interpolator
     * Otherwise it will just set the position of the tranform to newPosition
     */
    public void OnNewPositionRecieved(Vector2 newPosition)
    {
        if (Interpolate && interpolation != null)
        {
            interpolation.SetRecievedPosition(newPosition);
            return;
        }
        transform.position = newPosition;
        return;
    }

    public void OnDrawGizmos()
    {
        foreach( RaycastHit2D hit2D in raycastHits)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(hit2D.point, new Vector3(1, 1, 0.1f));
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( hit2D.point , 0.1f);
        }
    }
}
