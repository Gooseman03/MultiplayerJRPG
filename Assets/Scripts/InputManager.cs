using UnityEngine;
using UnityEngine.InputSystem;

namespace Ladder.Input
{
    [DisallowMultipleComponent]
    public class InputManager  : MonoBehaviour
    {
        private static InputManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            moveAction = InputSystem.actions.FindAction("move");
            lookAction = InputSystem.actions.FindAction("look");
            attackAction = InputSystem.actions.FindAction("attack");
            interactAction = InputSystem.actions.FindAction("interact");
            sprintAction = InputSystem.actions.FindAction("sprint");
        }

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction interactAction;
        private InputAction sprintAction;

        private Vector2 lastMoveAction = new Vector2(0,0);

        // Use any of these methods to get the current input state
        public static Vector2 Move => instance.GetMove();
        public static Vector2 LastMove => instance.GetLastMove();
        public static Vector2 Look => instance.GetLook(); 
        public static bool Attack => instance.GetAttack(); 
        public static bool Interact => instance.GetInteract(); 
        public static bool Sprint => instance.GetSprint(); 

        private Vector2 GetMove()
        {
            Vector2 value = moveAction.ReadValue<Vector2>().normalized;
            if (value != lastMoveAction) lastMoveAction = value;
            return value;
        }
        private Vector2 GetLastMove()
        {
            return lastMoveAction;
        }
        private Vector2 GetLook()
        {
            return lookAction.ReadValue<Vector2>();
        }
        private bool GetAttack()
        {
            return attackAction.IsPressed();
        }
        private bool GetInteract()
        {
            return interactAction.IsPressed();
        }
        private bool GetSprint()
        {
            return sprintAction.IsPressed();
        }
    }
}