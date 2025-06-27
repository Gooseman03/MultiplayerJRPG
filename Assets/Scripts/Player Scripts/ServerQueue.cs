using System;
using System.Collections.Generic;
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
        public uint Id;
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
        public MessageBundle LastGoodMessage = new MessageBundle();
        public void SetGoodMessage(MessageBundle message)
        {
            LastGoodMessage = message;
        }

        private Dictionary<uint, Vector2> messageBuffer = new Dictionary<uint, Vector2>();
        public void TryAddMessageToBuffer(Vector2 ClientInputVector, uint MessageId)
        {
            if (MessageId <= owner.NetworkManager.ServerTime.Tick) { return; }
            messageBuffer.TryAdd(MessageId, ClientInputVector);
        }
        public bool TryGetMessageAt(uint messageId, out MessageBundle message)
        {
            message = new();
            if (messageBuffer.TryGetValue(messageId, out Vector2 messageVector))
            {
                message = new() { Id = messageId, Input = messageVector };
                return true;
            }
            return false;
        }
        public void RemoveMessageFromBuffer(uint MessageId)
        {
            messageBuffer.Remove(MessageId);
        }

        /*
        [SerializeField] private List<MessageBundle> MessageQueue = new();
        [SerializeField] private List<MessageBundle> HistoryQueue = new();
        public void AddMessageToQueue(Vector2 ClientInputVector, uint MessageId)
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
        */
    }
}
