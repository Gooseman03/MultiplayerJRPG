using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Ladder.PlayerMovementHelpers;

public class ClientInterpolation : Interpolation
{
    public bool EnableExtrapolation = true;
    private uint extrapolationAttempts = 0; // How many times Extrapolation has been used since last received position
    private Vector2 TempPosition = Vector2.zero;
    protected override void OnAddNewPosition()
    {
        if (extrapolationAttempts != 0)
        {
            TempPosition = controller.Queue[lastRanPositionIndex].Position;

            Inputs inputs = controller.Queue[lastRanPositionIndex];
            inputs.Position = interpolationTarget.position;
            controller.Queue[lastRanPositionIndex] = inputs;
            timer = 0;
            Debug.Log("New Input Received during Extrapolation... Resyncing");
        }
        extrapolationAttempts = 0;
    }
    protected override bool OnInterpolationReset()
    {
        // If we are at the end of messages sent by the server add 1 to the nextposition and try to extrapolate
        if (EnableExtrapolation && nextPositionIndex == newestMessageIndex)
        {
            // Check if we have the data to Extrapolate with
            if (!controller.Queue.ContainsKey(nextPositionIndex - 1))
            {
                pauseInterpolation = true;
                return false;
            }
            if( TempPosition != Vector2.zero)
            {
                Inputs inputs = controller.Queue[lastRanPositionIndex];
                inputs.Position = TempPosition;
                controller.Queue[lastRanPositionIndex] = inputs;
                TempPosition = Vector2.zero;
            }
            Extrapolation();
            lastRanPositionIndex = nextPositionIndex;
            nextPositionIndex++;
        }
        else
        {
            lastRanPositionIndex = nextPositionIndex;
            nextPositionIndex = newestMessageIndex;
        }
        return true;
    }
    protected override bool PreInterpolation()
    {
        if (!EnableExtrapolation) { return true; }
        if (extrapolationAttempts >= 3)
        {
            pauseInterpolation = true;
            Debug.Log("Extrapolated to many times... Pausing");
            return false;
        }
        return true;
    }
    
    // Extrapolates and adds the value to the positions
    private void Extrapolation()
    {
        // Extrapolate next position by finding the delta movement and adding it to the current position
        Vector2 extrapolatedPosition = 2 * controller.Queue[nextPositionIndex].Position - controller.Queue[nextPositionIndex - 1].Position;
        Debug.Log("Input wasnt on time... Extrapolating");
        // Add the extrapolated input
        controller.Queue.TryAdd(nextPositionIndex + 1, new Inputs(extrapolatedPosition, controller.Queue[nextPositionIndex].IsAttacking));
        extrapolationAttempts++;
    }
}