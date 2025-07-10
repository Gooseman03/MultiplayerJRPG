using UnityEngine;
using UnityEngine.Events;

public class PuzzleComponent : MonoBehaviour
{
    [HideInInspector] public UnityEvent<PuzzleComponent> OnActivation;
    [HideInInspector] public UnityEvent<PuzzleComponent> OnDeactivation;
}