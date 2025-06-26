using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using static UnityEngine.InputSystem.InputRemoting;

namespace Ladder.PlayerMovementHelpers
{
    public struct MessageBundle
    {
        public Vector2 Input;
        public ulong Id;
    }
    public class ServerQueue
    {
        private PlayerMovement owner = null;
        public ServerQueue(PlayerMovement owner)
        {
            this.owner = owner;
        }

        [SerializeField] private List<MessageBundle> MessageQueue = new();
        public void AddMessageToQueue(Vector2 ClientInputVector, ulong MessageId)
        {
            MessageQueue.Add(new() { Id = MessageId, Input = ClientInputVector });
        }
        public bool GrabNextMessage(out MessageBundle message)
        {
            if (MessageQueue.Count == 0)
            {
                message = new();
                return false;
            }
            message = MessageQueue.First();
            MessageQueue.RemoveAt(0);
            return true;
        }
        public void ForEach(Action<MessageBundle> action)
        {
            MessageQueue.ForEach(action);
        }
    }
}
