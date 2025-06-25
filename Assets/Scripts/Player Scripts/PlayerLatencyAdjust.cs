using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.MovementAdjust
{
    [Serializable]
    public struct PositionStamp
    {
        public Vector2 Position;
        public double Time;
        public PositionStamp(Vector2 position, double time)
        {
            this.Position = position;
            this.Time = time;
        }
    }

    public class PlayerLatencyAdjust
    {
        private PlayerMovement movement;
        public PlayerLatencyAdjust (PlayerMovement owner)
            {
                movement = owner;
            }

        Vector2 LatencyAdjust = Vector2.zero;
        [SerializeField] private PositionStamp[] PreviousPositions = new PositionStamp[60];
        int PositionsCounter = 0;
        public void Stamp(Vector2 position , double time)
        {
            PreviousPositions[PositionsCounter] = new PositionStamp
                (
                position,
                time
                );
            
            PositionsCounter++;
            if (PositionsCounter >= 59)
            {
                PositionsCounter = 0;
            }
        }
        public bool Find(double time, out PositionStamp returnStamp)
        {
            returnStamp = new PositionStamp(new Vector2(),0);
            foreach (PositionStamp stamp in PreviousPositions)
            {
                Debug.Log(stamp.Time + " " + time);
                if (stamp.Time == time)
                {
                    returnStamp = stamp;
                    return true;
                }
            }
            return false;
        }
        public void ShiftStamps(Vector2 vector2)
        {
            PreviousPositions.ToList().ForEach((stamp) =>
            {
                stamp.Position += vector2;
            });
        }
        public void SetLatency(Vector2 position, double MessageTime)
        {
            PositionStamp stamp;
            if (Find(MessageTime, out stamp))
            {
                LatencyAdjust = position - stamp.Position;
            }
        }

        public void ApplyShift()
        {
            movement.PlayerMove(LatencyAdjust);
            ShiftStamps(LatencyAdjust);
            LatencyAdjust = Vector2.zero;
        }
    }
    
}

