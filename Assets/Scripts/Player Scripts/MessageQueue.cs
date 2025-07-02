using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace Ladder.PlayerMovementHelpers
{
    [Serializable]
    public struct MessageBundle : INetworkSerializable
    {
        public uint Id;
        public Inputs Inputs;
        
        public bool IsAttacking
        {
            get => Inputs.IsAttacking;
            set => Inputs.IsAttacking = value;
        }
        public Vector2 MoveInput
        {
            get
            {
                return Inputs.MoveInput;
            }
            set
            {
                Inputs.MoveInput = value;
            }
        }
        public MessageBundle(MessageBundle ToCopy)
        {
            Inputs = new Inputs();
            this.Id = ToCopy.Id;
            MoveInput = ToCopy.MoveInput;
            IsAttacking = ToCopy.IsAttacking;
        }
        public MessageBundle(uint Id, Vector2 vector , bool isAttacking)
        {
            Inputs = new Inputs();
            this.Id = Id;
            MoveInput = vector;
            IsAttacking = isAttacking;
        }
        public MessageBundle(uint Id, Inputs inputs)
        {
            this.Id = Id;
            this.Inputs = inputs;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Inputs);
        }
    }
    public struct Inputs : INetworkSerializable
    {
        private float x;
        private float y;
        public bool IsAttacking;
        public Vector2 Position
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
        public Vector2 MoveInput
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
        public Inputs(Vector2 vector, bool isAttacking)
        {
            this.x = vector.x;
            this.y = vector.y;
            IsAttacking = isAttacking;
        }

        public Inputs(Inputs ToCopy)
        {
            this.x = ToCopy.x;
            this.y = ToCopy.y;
            IsAttacking = ToCopy.IsAttacking;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref IsAttacking);
        }
    }
    public class MessageQueue
    {
        public Dictionary<uint, Inputs> messageBuffer = new Dictionary<uint, Inputs>(); // Dictionary of Messages Received the key is the messageId

        public Inputs this[uint key]
        {
            get => messageBuffer[key];
            set => messageBuffer[key] = value;
        }

        public MessageQueue(uint id,Inputs inputsInit)
        {
            messageBuffer[id] = inputsInit;
        }
        public MessageQueue() { }

        /* 
         * Takes in the current Tick and attempts to add more messages to the buffer
         * If the message is already in the buffer or if the server is past the intended tick for it will be dicarded
         */
        public void TryAddMessageToBuffer(uint time, MessageBundle message)
        {
            if (message.Id <= time) { return; }
            messageBuffer.TryAdd(message.Id, message.Inputs);
        }
        // Forces the message into the buffer overwriting previously held Key
        public void ForceAddMessageToBuffer(MessageBundle message)
        {
            messageBuffer[message.Id] = message.Inputs;
        }
        /* 
         * Takes in a index and outputs the message if it exists in the buffer.
         * If it cant find the index it will return false. 
         */
        public bool TryGetInput(uint messageId, out Inputs inputs)
        {
            inputs = new();
            if (messageBuffer.TryGetValue(messageId, out Inputs foundInputs))
            {
                inputs = foundInputs;
                return true;
            }
            return false;
        }

        public bool TryAdd(uint messageId, Inputs input)
        {
            if (messageBuffer.TryAdd(messageId, input))
            {
                return true;
            }
            return false;
        }
        // Will remove the message from the buffer at the index supplied
        public void RemoveMessageFromBuffer(uint MessageId)
        {
            messageBuffer.Remove(MessageId);
        }

        public bool ContainsKey(uint key) => messageBuffer.ContainsKey(key); 
    }
}
