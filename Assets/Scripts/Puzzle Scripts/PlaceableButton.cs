using UnityEngine;
using UnityEngine.Events;

// Client and Server side 
// Client just uses for visuals
public class PlaceableButton : PuzzleComponent
{
    private bool isLocked;
    [SerializeField] private bool isToggle;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLocked)
        {
            return;
        }
        if (isToggle)
        {
            isLocked = true;
        }
        OnActivation?.Invoke(this);
        GetComponentInChildren<SpriteRenderer>().color = Color.green;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isLocked || isToggle)
        {
            return;
        }
        OnDeactivation?.Invoke(this);
        GetComponentInChildren<SpriteRenderer>().color = Color.red;
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
