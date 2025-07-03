using UnityEngine;
using Ladder.Multiplayer.Multiplayer.Syncing;

[RequireComponent(typeof(Multiplayer))]
public class debugmultiplayerExample : MonoBehaviour
{
    Multiplayer multiplayerSyncing = null;
    [SerializeField] private bool isPuzzleCompleted = false;
    private bool hasTriggeredActivation = false;

    private void Start()
    {
        multiplayerSyncing = GetComponent<Multiplayer>();
        multiplayerSyncing.AtTriggerTime += OnActivation;
    }

    private void OnActivation()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void Update()
    {
        if (isPuzzleCompleted && !hasTriggeredActivation)
        {
            WhenPuzzleCompleted();
            hasTriggeredActivation = true;
        }
    }

    private void WhenPuzzleCompleted()
    {
        // These are the same just different ways of counting
        multiplayerSyncing.TicksToDelay = 150;
        // multiplayerSyncing.SecondsToDelay = 5;
        multiplayerSyncing.StartTimer();
    }
}
