using UnityEngine;
using Ladder.Multiplayer.Multiplayer.Syncing;

[RequireComponent(typeof(Multiplayer))]
public class debugmultiplayerExample : MonoBehaviour
{
    [SerializeField] private bool isPuzzleCompleted = false;
    private bool hasTriggeredActivation = false;
    Multiplayer multiplayerSyncing = null;
    
    // This is a example of how to use the multiplayer syncing component
    private void Start()
    {
        // Get the Multiplayer component attached to this GameObject.
        multiplayerSyncing = GetComponent<Multiplayer>();

        // Subscribe to the AtTriggerTime event. This will call OnActivation when the timer is up.
        multiplayerSyncing.AtTriggerTime.AddListener(OnActivation);
    }
    private void WhenPuzzleCompleted()
    {
        // Set the number of ticks to delay the event. 
        // This delay corresponds to how long the system should wait before triggering the event. (150 ticks = delay)
        multiplayerSyncing.TicksToDelay = 150;

        // Alternatively, you can set the delay in seconds. Uncomment the next line to do that:
        // multiplayerSyncing.SecondsToDelay = 5; 

        // Start the timer to trigger the event after the delay.
        multiplayerSyncing.StartTimer();
    }
    private void Update()
    {
        if (isPuzzleCompleted && !hasTriggeredActivation)
        {
            WhenPuzzleCompleted();
            hasTriggeredActivation = true;
        }
    }
    // This is the method that gets called when the trigger time arrives (after the delay). This will be activated on all clients at the same time
    private void OnActivation()
    {
        // This is a super simple Example of what could be done. This could be anything.
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
    }
}
