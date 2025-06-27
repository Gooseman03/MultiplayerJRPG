using Ladder.Input;
using Unity.Netcode;
using UnityEngine;

public class PredictPlayerMovement : NetworkBehaviour
{

    [SerializeField] private float speed;

    private Rigidbody2D rb;

    private Vector2 lastInput = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //rb.linearVelocity = Vector2.zero;
        if (!IsLocalPlayer) return;
        Move();
    }

    private void FixedUpdate()
    {
        //if (!IsLocalPlayer) return;
    }

    private void Move()
    {
        Vector2 inputVector = InputManager.Move.normalized;
        if (!IsServer && lastInput != inputVector)
        {
            MovementRPC(inputVector);
            lastInput = inputVector;
        }
        rb.linearVelocity = inputVector * speed;
    }

    [Rpc(SendTo.Server)]
    private void MovementRPC(Vector2 inputVector)
    {
        //Vector2 moveVector = movementVector.normalized * speed * Time.fixedDeltaTime;
        //rb.MovePosition(rb.position + moveVector);
        rb.linearVelocity = inputVector * speed;
    }
}
