using Ladder.PlayerMovementHelpers;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Interpolation : MonoBehaviour
{
    public Dictionary<uint,Vector2> Queue = new Dictionary<uint, Vector2>();

    [Tooltip("This will Enable/Disable Interpolation If it is Disabled the transform will be set directly")]
    public bool EnableInterpolation = true;

    protected uint newestMessageIndex = 0;      // The latest message received by the server

    protected uint nextPositionIndex = 0;       // What is the next Postition to lerp to
    protected uint lastRanPositionIndex = 0;    // What was the last position we lerped to
    private float tickLength;                   // How long a Tick is used for the lerp timing

    protected bool pauseInterpolation = true;   // Whether interpolation should continue
    protected Transform interpolationTarget;

    protected float timer = 0; // Lerp Timer

    // Add a new Position that was sent from the server
    public void AddNewPosition(uint messageId, Vector2 vector)
    {
        if (!EnableInterpolation)
        {
            transform.position = vector;
            return;
        }

        Queue[messageId] = vector;
        newestMessageIndex = messageId;

        // Reset interpolation state if its paused and we receive a new message
        if (pauseInterpolation)
        {
            timer = 0;
            nextPositionIndex = messageId;
            pauseInterpolation = false;
        }
        OnAddNewPosition();
    }
    public void ForcePosition(Vector2 vector2)
    {
        pauseInterpolation = true;
        interpolationTarget.position = vector2;
    }
    public void Start()
    {
        //Queue ??= new Dictionary<uint, Vector2>(); // If Queue is null make one ??= does that
        Queue.Add(0, new Vector2(0, 0));
        interpolationTarget = gameObject.transform;
        PostStart();
    }
    public void Update()
    {
        if (!EnableInterpolation) { return; }

        tickLength = NetworkManager.Singleton.ServerTime.FixedDeltaTime;

        // If pauseInterpolation has been set wait until a new postition is received from the server
        if (pauseInterpolation) { return; }

        // Let child class run logic before interpolation; if it returns false, stop.
        if (!PreInterpolation()) return;

        timer += Time.deltaTime;
        if (timer > tickLength)
        {
            // Let child re-check after reset, in case they want to skip this frame.
            if (!OnInterpolationReset()) return;
            timer = 0;
        }
        PerformInterpolation();
    }
    private void PerformInterpolation()
    {
        if (Queue.ContainsKey(nextPositionIndex) && Queue.ContainsKey(lastRanPositionIndex))
        {
            if (Queue[nextPositionIndex] - Queue[lastRanPositionIndex] == Vector2.zero)
            {
                // No movement required — wait for server.
                pauseInterpolation = true;
                return;
            }
            interpolationTarget.position = FindNextMove(lastRanPositionIndex, nextPositionIndex);
        }
        else
        {
            Debug.LogError("A value was missing during interpolation... Pausing Interpolation");
            // Something went wrong a value is missing - wait for server
            pauseInterpolation = true;
            return;
        }
    }


    protected virtual void PostStart() { }
    /// <summary>
    /// <para>Called when the interpolation timer resets.</para>
    /// <para>Base implementation updates the position indices.</para>
    /// <para>Return <c>false</c> to skip interpolation this frame.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// lastRanPositionIndex = nextPositionIndex;
    /// nextPositionIndex = newestMessageIndex;
    /// </code>
    /// </example>
    protected virtual bool OnInterpolationReset()
    {
        lastRanPositionIndex = nextPositionIndex;
        nextPositionIndex = newestMessageIndex;
        return true;
    }
    protected virtual void OnAddNewPosition() { }
    /// <summary>
    /// <para>Called by the base class before interpolation occurs.</para>
    /// <para>Return <c>false</c> to interrupt interpolation for this frame.</para>
    /// </summary>
    protected virtual bool PreInterpolation() => true;
    // Returns the nextposition to move to by using a lerp
    private Vector2 FindNextMove(uint from, uint to)
    {
        Vector2 start = Queue[from];
        Vector2 end = Queue[to];
        float t = timer / tickLength;
        return Vector2.Lerp(start, end, t);
    }
}
