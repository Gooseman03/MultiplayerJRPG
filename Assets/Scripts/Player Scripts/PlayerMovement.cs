using Ladder.Input;
using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;

    // Collision Variables
    private List<RaycastHit2D> raycastHits = new List<RaycastHit2D>(); 
    [SerializeField] private LayerMask collisionMask; // What layers the player can collide with
    [SerializeField] private float skinWidth = 0.1f;  // A Extra Buffer when doing collision checks

    // Moves the player as by the vector input and if there is a collision stop there
    public void MovePlayerWithCollisions(Vector2 movementVector)
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
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.localScale, 0f, move.normalized, move.magnitude + skinWidth, collisionMask);
        return hit.collider != null;
    }

    private float GetSafeMoveDistance(Vector2 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.localScale, 0f, move.normalized, move.magnitude + skinWidth, collisionMask);
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
}
