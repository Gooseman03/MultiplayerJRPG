using UnityEngine;
using UnityEngine.Events;

public class PuzzleComponent : MonoBehaviour
{
    public UnityEvent<PuzzleComponent> OnActivation;
    public UnityEvent<PuzzleComponent> OnDeactivation;
}