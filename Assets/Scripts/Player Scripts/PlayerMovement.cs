using Ladder.Input;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;

    private Rigidbody2D rb;
    //public SpriteRenderer sr;
    //public Animator animator;
    //public ParticleSystem particle;

    private bool canRoll; // Use to determine a timer on how often you can roll
    private bool isRolling; // Determines if the player is currently rolling

    [SerializeField] private float rollSpeed; // Use to determine how fast / far the roll will take you
    [SerializeField] private float rollTimer; // How long until you can roll again
    [SerializeField] private float rollDuration; // How long the roll state should last
    [SerializeField] private float rollCooldown; // The countdown for rolling (Works with rollTimer)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //particle.Stop();
    }
    
    private void Update()
    {
        // Server & Localplayer will Run this to keep track of timers
        if (!IsLocalPlayer) return;
        rb.linearVelocity = Vector2.zero;
        if (!canRoll)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0)
            {
                canRoll = true;
            }
        }
    }
    
    void FixedUpdate()
    {
        if (!IsLocalPlayer) return;
        // Server & Localplayer will run this
        if (!isRolling)
        {
            //animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            //rb.linearVelocity = InputManager.Move * speed;

            Vector2 movevector = InputManager.Move;
            if (movevector != Vector2.zero)
            {
                MovementRPC(movevector, InputManager.Sprint);
            }
        }
    }

    private void Movement()
    {
        //if (movementVector != Vector2.zero)
        //{
        //    animator.SetFloat("XInput", movement.x);
        //    sr.flipX = movement.x > 0;
        //    animator.SetFloat("YInput", movement.y);
        //}
        //if (InputManager.Sprint && canRoll && InputManager.LastMove != Vector2.zero)
        //{
        //    StartCoroutine(PerformRoll());
        //}
    }

    //private IEnumerator PerformRoll()
    //{
    //    
    //    //particle.Play();
    //    canRoll = false;
    //    isRolling = true;
    //    //animator.SetBool("IsRolling", true);
    //    // Sets the roll direction based on the movement vector unless it's zero, in which case it uses the last movement direction
    //    Vector2 rollDirection = movementVector != Vector2.zero ? movementVector : InputManager.LastMove;
    //    float rollEndTime = Time.time + rollDuration;
    //
    //    while (Time.time < rollEndTime)
    //    {
    //        rb.linearVelocity = rollDirection * rollSpeed;
    //        yield return null;
    //    }
    //    //particle.Stop();
    //    //animator.SetBool("IsRolling", false);
    //    isRolling = false;
    //    rollTimer = rollCooldown;
    //}

    [Rpc(SendTo.Server)]
    private void MovementRPC(Vector2 movementVector, bool roll)
    {
        // Normalized incase player input is invalid
        Vector2 moveVector = movementVector.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveVector);
    }
}
