using Ladder.PlayerMovementHelpers;
using Unity.Netcode;
using UnityEngine;

public class ClientTimeDilation : NetworkBehaviour
{
    public float CurrentTickOffset = 0;
    private int messageRedundancy = 0;

    
}
