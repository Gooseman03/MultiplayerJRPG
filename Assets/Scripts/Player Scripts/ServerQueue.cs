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
        public Inputs(Vector2 movementVector, bool isAttacking)
        {
            this.x = movementVector.x;
            this.y = movementVector.y;
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
    public class ServerQueue
    {
        private Dictionary<uint, Inputs> messageBuffer = new Dictionary<uint, Inputs>(); // Dictionary of Messages Recieved from the client the key is the messageId
        public MessageBundle LastGoodMessage = new MessageBundle(); // Contains the Last Message that was successfully ran
        /* 
         * Takes in the tick of the when the message was sent and a Set of Inputs
         * Creates a MessageBundle with those and stores it in LastGoodMessage
        */
        public void SetGoodMessage(uint time,Inputs inputs)
        {
            LastGoodMessage = new MessageBundle(time, inputs);
        }
        
        /* 
         * Takes in the current Tick and attempts to add more messages to the buffer
         * If the message is already in the buffer or if the server is past the intended tick for it will be dicarded
         */
        public void TryAddMessageToBuffer(uint time, MessageBundle message)
        {
            if (message.Id <= time) { return; }
            messageBuffer.TryAdd(message.Id, message.Inputs);
        }
        /* 
         * Takes in a index and outputs the message if it exists in the buffer.
         * If it cant find the index it will return false. 
         */
        public bool TryGetInputsAt(uint messageId, out Inputs inputs)
        {
            inputs = new();
            if (messageBuffer.TryGetValue(messageId, out Inputs foundInputs))
            {
                inputs = foundInputs;
                return true;
            }
            return false;
        }

        // Will remove the message from the buffer at the index supplied
        public void RemoveMessageFromBuffer(uint MessageId)
        {
            messageBuffer.Remove(MessageId);
        }
    }
}
