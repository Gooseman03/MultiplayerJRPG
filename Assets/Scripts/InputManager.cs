using Ladder.Input;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ladder.Input
{
    [DisallowMultipleComponent]
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance = null;

        private InputManager() { }

        public static InputManager Singleton
        {
            get
            {
                if (instance == null)
                {
                    instance = new InputManager();
                }
                return instance;
            }
        }
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction interactAction;

        private void Awake()
        {
            moveAction = InputSystem.actions.FindAction("move");
            lookAction = InputSystem.actions.FindAction("look");
            attackAction = InputSystem.actions.FindAction("attack");
            interactAction = InputSystem.actions.FindAction("interact");
        }
        public Vector2 GetMove()
        {
            return moveAction.ReadValue<Vector2>();
        }
        public Vector2 GetLook()
        {
            return lookAction.ReadValue<Vector2>();
        }
        public bool GetAttack()
        {
            return attackAction.ReadValue<bool>();
        }
        public bool GetInteract()
        {
            return interactAction.ReadValue<bool>();
        }
    }
}