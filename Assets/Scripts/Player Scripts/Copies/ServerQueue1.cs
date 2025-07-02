using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Dependencies have changed to much to keep working for now

//namespace Ladder.PlayerMovement1Helpers
//{
//
//    /// <summary>
//    /// Struct that represents a message that is sent between a client and a server.<para></para>
//    /// <param>Vector2 Input - A Vector2 recording the input provided by the client.</param><br></br>
//    /// <param>int IntendedTick - The tick this input was registered on.</param>
//    /// </summary>
//    [Serializable]
//    public struct MessageBundle1
//    {
//        private float x;
//        private float y;
//        public Vector2 Input
//        {
//            get
//            {
//                return new Vector2(x, y);
//            }
//            set
//            {
//                x = value.x; 
//                y = value.y;
//            }
//        }
//        public int IntendedTick { get; set; }
//        public MessageBundle1(MessageBundle1 ToCopy)
//        {
//            this.IntendedTick = ToCopy.IntendedTick;
//            x = ToCopy.Input.x;
//            y = ToCopy.Input.y;
//        }
//    }
//
//    public class ServerQueue1
//    {
//        private PlayerMovement1 owner = null;
//        public ServerQueue1(PlayerMovement1 owner)
//        {
//            this.owner = owner;
//        }
//
//        [SerializeField] private Dictionary<int, MessageBundle1> MessageQueue = new();
//        [SerializeField] private MessageBundle1 LastMessage = new();
//
//        public void AddMessageToQueue(int DesiredTick, Vector2 ClientInputVector)
//        {
//            MessageQueue.TryAdd
//                (
//                DesiredTick,
//                new()
//                {
//                    IntendedTick = DesiredTick, Input = ClientInputVector
//                });
//            //owner.serverBacklog = MessageQueue.Keys.ToArray();
//        }
//
//        public bool GetMessage(int tick, out MessageBundle1 message)
//        {
//            if (MessageQueue.TryGetValue(tick, out message))
//            {
//                LastMessage = new(message);
//                MessageQueue.Remove(tick);
//                //owner.serverBacklog = MessageQueue.Keys.ToArray();
//                return true;
//            }
//            //owner.serverBacklog = MessageQueue.Keys.ToArray();
//            return false;
//        }
//
//        public MessageBundle1 GetLastSuccessfulMessage()
//        {
//            return LastMessage;
//        }
//
//    }
//}
