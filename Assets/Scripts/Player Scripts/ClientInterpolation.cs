using Unity.Netcode;
using UnityEngine;

public class ClientInterpolation : NetworkBehaviour
{
    private Vector3 recievedPosition = Vector3.zero;
    private Vector3 transformWhenRecieved = Vector3.zero;
    private float time = 0;

    [SerializeField] private float InterpolationSpeed = 10;
    public void SetRecievedPosition(Vector3 newPosition)
    {
        recievedPosition = newPosition;
        transformWhenRecieved = transform.position;
        time = 0;
    }
    public Vector3 FindNextMove()
    {
        time += Time.deltaTime * InterpolationSpeed;
        return Vector3.Lerp(transformWhenRecieved, recievedPosition, time);
    }
}