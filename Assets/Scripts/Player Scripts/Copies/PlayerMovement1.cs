using Ladder.Input;
using Ladder.PlayerMovement1Helpers;
using Unity.Netcode;
using UnityEngine;

//Still have yet to work on this one.
public class PlayerMovement1 : NetworkBehaviour
{
    /*[SerializeField] public int[] clientBacklog = null;
    [SerializeField] public int[] serverBacklog = null;*/

    [SerializeField] private float speed;
    [SerializeField] private int tickBuffer = 10;

    private ClientReconcile1 ClientReconcile1 = null;
    private ServerQueue1 ServerQueue1 = null;
    private ClientInterpolation interpolation = null;
    private ServerInputAssumption extrapolation = null;
    [SerializeField] private bool Interpolate = false;
    [SerializeField] private bool Extrapolate = false;

    private void Awake()
    {
        ClientReconcile1 = GetComponent<ClientReconcile1>();
        ServerQueue1 = new ServerQueue1(this);
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

    private void Update()
    {
        
    }

    private void OnTick()
    {
        if (IsServer)
        {
            if (IsLocalPlayer)
            {
                Vector2 MovementVector = InputManager.Move.normalized;
                if (MovementVector != Vector2.zero)
                {
                    Movement(MovementVector);
                }
                UpdatePositionRPC(0, transform.position);
            }
            else
            {
                MessageBundle1 message = new();
                if (!ServerQueue1.GetMessage(NetworkManager.ServerTime.Tick, out message))
                {
                    // Will simply redo the last seen message until client is caught up
                    message = ServerQueue1.GetLastSuccessfulMessage();
                }
                //if (message.Id == 0) return;
                Movement(message.Input.normalized);
                UpdatePositionRPC(message.IntendedTick, transform.position);
            }
        }
        else
        {
            if (IsLocalPlayer)
            {
                Vector2 MovementVector = InputManager.Move.normalized;
                if (MovementVector != Vector2.zero)
                {
                    Movement(MovementVector);
                }
                ClientReconcile1.StampLocation(NetworkManager.LocalTime.Tick+tickBuffer, MovementVector);
                ClientReconcile1.GetPreviousMessages(out int[] ticks, out float[] xs, out float[] ys);
                MovementRequestRPC(ticks, xs, ys);
            }
            else
            {
                if (Interpolate && interpolation != null)
                {
                    transform.position = interpolation.FindNextMove();
                }
            }
        }
    }

    //Assumes Vector2 will be normalized in the rpc
    public void Movement(Vector2 movementVector)
    {
        // Length of one tick
        float deltaTime = NetworkManager.Singleton.NetworkTickSystem.ServerTime.FixedDeltaTime;
        Vector2 moveVector = movementVector * speed * deltaTime;
        transform.position = new(
            transform.position.x + moveVector.x,
            transform.position.y + moveVector.y,
            0
            );
    }

    [Rpc(SendTo.NotServer)]
    public void UpdatePositionRPC(int IntendedTick, Vector2 NewPosition)
    {
        if(IsLocalPlayer)
        {
            ClientReconcile1.IsPredictionCorrect(IntendedTick, NewPosition);
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
    public void MovementRequestRPC(int[] ticks, float[] xs, float[] ys)
    {
        for (int i = 0; i < ticks.Length; i++)
        {
            if (NetworkManager.LocalTime.Tick < ticks[i]) ServerQueue1.AddMessageToQueue(ticks[i], new(xs[i], ys[i]));
        }
    }
}
