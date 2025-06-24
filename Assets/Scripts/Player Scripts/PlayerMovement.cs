using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
        public float speed;
        public Vector2 movement;
        private Rigidbody2D rb;
        //public SpriteRenderer sr;
        //public Animator animator;
        //public ParticleSystem particle;

        private bool canRoll; //Use to determine a timer on how often you can roll
        private bool isRolling; //Determines if the player is currently rolling
    
        public float rollSpeed; //Use to determine how fast / far the roll will take you
        public float rollTimer; //How long until you can roll again
        public float rollDuration; //How long the roll state should last
        public float rollCooldown; //The countdown for rolling (Works with rollTimer)
    
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            //particle.Stop();
        }
    
        private void Update()
        {
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
            if (!isRolling)
            {
                //animator.SetFloat("Speed", rb.linearVelocity.magnitude);
                rb.linearVelocity = movement * speed;
            }
        }

        public void Move(InputAction.CallbackContext context)
        {
            movement = context.ReadValue<Vector2>();

            if (movement != Vector2.zero)
            {
                //animator.SetFloat("XInput", movement.x);
                //sr.flipX = movement.x > 0;
                //animator.SetFloat("YInput", movement.y);
            }
        }

        public void Sprint(InputAction.CallbackContext context)
        {
            if (context.started && canRoll && movement != Vector2.zero)
            {
                StartCoroutine(PerformRoll());
            }
        }

        private IEnumerator PerformRoll()
        {
            //particle.Play();
            canRoll = false;
            isRolling = true;
            //animator.SetBool("IsRolling", true);
            Vector2 rollDirection = movement.normalized;
            float rollEndTime = Time.time + rollDuration;

            while (Time.time < rollEndTime)
            {
                rb.linearVelocity = rollDirection * rollSpeed;
                yield return null;
            }
            //particle.Stop();
            //animator.SetBool("IsRolling", false);
            isRolling = false;
            rollTimer = rollCooldown;
        }
}
