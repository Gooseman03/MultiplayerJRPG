using Ladder.Multiplayer.Multiplayer.Syncing;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleController : NetworkBehaviour
{
    [SerializeField] private List<PuzzleComponent> RequiredObjects = new List<PuzzleComponent>();
    [SerializeField] private Multiplayer multiplayer;
    [SerializeField] private bool IsPuzzleComplete = false;
    [SerializeField] private Dictionary<PuzzleComponent, bool> puzzleChecks = new();

    private void Awake()
    {
        foreach (PlaceableButton button in RequiredObjects)
        {
            puzzleChecks.Add(button,false);
            // If the Button is pressed set its value in the puzzle dictionary to true and see if all values are true if they are complete the puzzle
            button.OnActivation.AddListener((PuzzleComponent invoker) => 
            {
                puzzleChecks[invoker] = true;
                foreach (bool value in puzzleChecks.Values)
                {
                    if (value == false)
                    {
                        return;
                    }
                }
                OnPuzzleCompleted();
            });
            // If the button is released Set its value in the dictionary to false
            button.OnDeactivation.AddListener((PuzzleComponent invoker) =>
            {
                puzzleChecks[invoker] = false; 
            });
        }
    }
    public void OnPuzzleCompleted()
    {
        if (IsClient || IsPuzzleComplete)
        {
            return;
        }
        IsPuzzleComplete = true;
        multiplayer.StartTimer();
    }
}
