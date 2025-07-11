using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Events;

// Client and Server side 
// Client just uses for visuals
public class PlaceableButton : PuzzleComponent
{
    private bool isLocked;
    [SerializeField] private bool isToggle;
    private int CountObjectsColliding = 0;
    private bool activated;
    private void OnTriggerEnter2D(Collider2D collision)
    { 
        if (isLocked)
        {
            return;
        }
        if (collision.CompareTag("Player") || collision.GetComponent<PushableObject>())
        {
            CountObjectsColliding++;
            if (isToggle)
            {
                isLocked = true;
            }
            if (CountObjectsColliding > 0 && !activated)
            {
                activated = true;
                OnActivation?.Invoke(this);
                GetComponentInChildren<SpriteRenderer>().color = Color.green;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isLocked || isToggle)
        {
            return;
        }
        if (collision.CompareTag("Player") || collision.GetComponent<PushableObject>())
        {
            CountObjectsColliding--;
            if (CountObjectsColliding == 0 && activated)
            {
                activated = false;
                OnDeactivation?.Invoke(this);
                GetComponentInChildren<SpriteRenderer>().color = Color.red;
            }
        }
    }
    public void DebugLockAsPressed()
    {
        OnActivation?.Invoke(this);
        GetComponentInChildren<SpriteRenderer>().color = Color.green;
        isLocked = true;
    }
    public void DebugUnlockAsPressed()
    {
        OnDeactivation?.Invoke(this);
        GetComponentInChildren<SpriteRenderer>().color = Color.red;
        isLocked = false;
    }
}
