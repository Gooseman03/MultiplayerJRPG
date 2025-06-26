using Ladder.PlayerMovementHelpers;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.PlayerMovementHelpers
{
    [Serializable]
    public struct MessageBundle
    {
        private float x;
        private float y;
        public Vector2 Input
        {
            get
            {
                return new Vector2(x, y);
            }
            set
            {
                x = value.x; 
                y = value.y;
            }
        }
        public ulong Id;
        public MessageBundle(MessageBundle ToCopy)
        {
            this.Id = ToCopy.Id;
            x = ToCopy.Input.x;
            y = ToCopy.Input.y;
        }
    }
    public class ServerQueue
    {
        private PlayerMovement owner = null;
        public ServerQueue(PlayerMovement owner)
        {
            this.owner = owner;
        }

        [SerializeField] private List<MessageBundle> MessageQueue = new();
        [SerializeField] private List<MessageBundle> HistoryQueue = new();
        public void AddMessageToQueue(Vector2 ClientInputVector, ulong MessageId)
        {
            MessageQueue.Insert(MessageQueue.FindLastIndex((mes) => mes.Id <= MessageId) + 1,
                new() {
                    Id = MessageId, Input = ClientInputVector 
                });
        }
        public List<MessageBundle> GetMessageQueue()
        {
            List<MessageBundle> queue = new(MessageQueue);
            MessageQueue.Clear();
            HistoryQueue.AddRange(queue);
            return queue;
        }
        public List<MessageBundle> GetHistoryQueue()
        {
            List<MessageBundle> queue = new(HistoryQueue);
            // Will need to be cleared at some point. Otherwise look forwards to memory leaks
            // Look into making a replay file where the contents of HistoryQueue are stored in a file which is regularly updated to keep memory usage low
            return queue;
        }
        public void ForEach(Action<MessageBundle> action)
        {
            MessageQueue.ForEach(action);
        }
    }
}
