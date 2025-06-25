using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.MovementQueue
{
    public struct MovementAction
    {
        public Vector2 Move { get; }
        public double Tick;
        public MovementAction(Vector2 move, double time)
        {
            Move = move;
            Tick = time;
        }
    }
    public class ServerMovementQueue
    {
        float MaxTickDifference = 0.50f;
        private List<MovementAction> Queue = new List<MovementAction>();
        
        public void RemoveStalePackets()
        {
            List<int> LatePackets = new List<int>();
            for (int i = 0; i < Queue.Count; i++)
            {
                if (Queue[i].Tick < NetworkManager.Singleton.ServerTime.Time - MaxTickDifference)
                {
                    LatePackets.Add(i);
                    continue;
                }
            }
            // Remove Late packets in reverse order as to not mess up list
            for (int i = LatePackets.Count - 1; i >= 0; i++)
            {
                Queue.RemoveAt(LatePackets[i]);
            }
        }

        public MovementAction? HandleOldestPacket()
        {
            if (Queue.Count == 0)
            {
                return null;
            }
            MovementAction output = Queue[0];
            Queue.RemoveAt(0);
            return output;
        }
        public void AddToQueue(MovementAction NewAction)
        {
            if (NewAction.Tick < NetworkManager.Singleton.ServerTime.Time - MaxTickDifference)
            {
                return;
            }
            int insertpoint = 0;
            Queue.ForEach((action) => { if (NewAction.Tick < action.Tick) return; insertpoint++; });
            Queue.Insert(insertpoint, NewAction);
        }
    }

}