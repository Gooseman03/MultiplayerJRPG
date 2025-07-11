using Ladder.Multiplayer.Multiplayer.Syncing;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defines the <see cref="PuzzleController" />
/// </summary>
public class PuzzleController : NetworkBehaviour
{
    /// <summary>
    /// Defines the RequiredObjects For the Puzzle to be completed
    /// </summary>
    [SerializeField] private List<PuzzleComponent> RequiredObjects = new List<PuzzleComponent>();

    /// <summary>
    /// Defines the multiplayer Timer
    /// </summary>
    [SerializeField] private Multiplayer multiplayer;

    /// <summary>
    /// Defines if the Puzzle Is Complete
    /// </summary>
    [SerializeField] private bool IsPuzzleComplete = false;

    /// <summary>
    /// Defines the puzzleChecks A dictionary and just is the requiredObject and a bool
    /// </summary>
    [SerializeField] private Dictionary<PuzzleComponent, bool> puzzleChecks = new();

    public override void OnNetworkSpawn()
    {
        foreach (PuzzleComponent section in RequiredObjects)
        {
            puzzleChecks.Add(section, false);

            // If the Button is pressed set its value in the puzzle dictionary to true
            section.OnActivation.AddListener((PuzzleComponent invoker) =>
            {
                puzzleChecks[invoker] = true;
                CheckPuzzleState();
            });

            // If the button is released Set its value in the dictionary to false
            section.OnDeactivation.AddListener((PuzzleComponent invoker) =>
            {
                puzzleChecks[invoker] = false;
                CheckPuzzleState();
            });
        }
    }

    /// <summary>
    /// The CheckPuzzleState checks to see if the <see cref="OnPuzzleCompleted" /> or the <see cref="OnPuzzleUncompleted" /> should be triggered
    /// </summary>
    private void CheckPuzzleState()
    {
        if (!IsServer)
        {
            return;
        }
        foreach (bool value in puzzleChecks.Values)
        {
            // If any of the Puzzle Parts arent done then see if that just happened and call OnPuzzleUncompleted() 
            if (value == false)
            {
                if (IsPuzzleComplete)
                {
                    OnPuzzleUncompleted();
                }
                return;
            }
        }
        // If they were all complete, and this is the first time then call OnPuzzleCompleted()
        if (!IsPuzzleComplete)
        {
            OnPuzzleCompleted();
        }
    }

    /// <summary>
    /// The OnPuzzleCompleted
    /// </summary>
    public void OnPuzzleCompleted()
    {
        IsPuzzleComplete = true;
        multiplayer.TicksToDelay = 30;
        multiplayer.StartTimer();
    }

    /// <summary>
    /// The OnPuzzleUncompleted
    /// </summary>
    public void OnPuzzleUncompleted()
    {
        IsPuzzleComplete = false;
        multiplayer.TicksToDelay = 3;
        multiplayer.StartTimer();
    }
}
