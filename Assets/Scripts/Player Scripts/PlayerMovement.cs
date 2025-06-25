using Ladder.Input;
using Ladder.MovementAdjust;
using Ladder.MovementQueue;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;

    [SerializeField] private GameObject baseGhost;
    private GameObject ghost;

    //public SpriteRenderer sr;
    //public Animator animator;
    //public ParticleSystem particle;

    private bool canRoll; // Use to determine a timer on how often you can roll
    private bool isRolling; // Determines if the player is currently rolling

    [SerializeField] private float rollSpeed; // Use to determine how fast / far the roll will take you
    [SerializeField] private float rollTimer; // How long until you can roll again
    [SerializeField] private float rollDuration; // How long the roll state should last
    [SerializeField] private float rollCooldown; // The countdown for rolling (Works with rollTimer)

    private ServerMovementQueue m_serverMovementQueue;
    private PlayerLatencyAdjust m_clientLatencyAdjust;
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        //particle.Stop();
        if (IsLocalPlayer)
        {
            m_clientLatencyAdjust = new PlayerLatencyAdjust(this);
        }
        if (IsServer || IsLocalPlayer)
        {
            ghost = Instantiate(baseGhost);
        }
        if (IsServer)
        {
            m_serverMovementQueue = new ServerMovementQueue();
        }
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            PositionGhostRPC(rb.position);
        }
    }
    public Vector2 PlayerMove(Vector2 vector)
    {
        Vector3 NextVector = new Vector2(
            vector.x + rb.position.x,
            vector.y + rb.position.y
            );
        rb.MovePosition(NextVector);
        //Debug.Log(new Vector2(NextVector.x - rb.position.x, NextVector.y - rb.position.y));
        return NextVector;
    }
    public void PlayerMoveServer(MovementAction action)
    {
        PlayerMove(action.Move);
    }
    

    int debugcounter = 0;
    void ServerMovementMessaging(Vector2 movevector, double messageTick)
    {
        MovementRPC(movevector, InputManager.Sprint, messageTick);
    }
    void FixedUpdate()
    {
        Debug.Log(NetworkManager.Singleton.ServerTime.Tick);
        if (IsServer)
        {
            m_serverMovementQueue.RemoveStalePackets();
            MovementAction? action = m_serverMovementQueue.HandleOldestPacket();
            if (action != null) 
            {
                PlayerMoveServer((MovementAction)action);
            }
            PositionUpdateRPC(rb.position, NetworkManager.Singleton.ServerTime.Tick);
        }
        if (!IsLocalPlayer) return;
        double messageTick = NetworkManager.Singleton.ServerTime.Tick;
        m_clientLatencyAdjust.Stamp(rb.position, messageTick);
        Movement(messageTick);
        if (IsHost)
        {
            PositionUpdateRPC(rb.position, 0);
        }
    }
    private void Movement(double messageTick)
    {
        Vector2 movevector = InputManager.Move.normalized;
        debugcounter++;
        //Debug.Log("count" + debugcounter + "Transform " + transform.position + " move amount " + (movevector * speed * .02f) + " rb.position " + rb.position + " Latency Adjust " + LatencyAdjust);
        m_clientLatencyAdjust.ApplyShift();
        if (movevector != Vector2.zero)
        {
            PlayerMove( movevector * speed * .02f);
            if (!IsServer)
            {
                ServerMovementMessaging(movevector, messageTick);
            }
        }
        
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void PositionGhostRPC(Vector2 position)
    {
        ghost.transform.position = position;
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void MovementRPC(Vector2 movementVector, bool roll, double tick, RpcParams rpcParams = new RpcParams())
    {
        Vector2 moveVector = movementVector.normalized * speed * .02f;
        m_serverMovementQueue.AddToQueue(new MovementAction(moveVector, tick));
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void PositionUpdateRPC(Vector2 NewPosition, double MessageTime)
    {
        if (IsLocalPlayer)
        {
            m_clientLatencyAdjust.SetLatency(NewPosition, MessageTime);
            ghost.transform.position = NewPosition;
        }
        else
        {
            rb.MovePosition(NewPosition);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(ghost.transform.position, new Vector3(1, 1, 1));
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

}
