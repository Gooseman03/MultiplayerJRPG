using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Ladder.PlayerMovementHelpers
{
    public class ClientReconcile : NetworkBehaviour
    {
        private Dictionary<ulong, Vector2> PreviousLocations = new Dictionary<ulong, Vector2>();
        private PlayerMovement owner = null;
        private ulong messageCounter = 0;
        private void Awake()
        {
            owner = GetComponent<PlayerMovement>();
        }
        public void StampLocation()
        {
            PreviousLocations.Add(messageCounter, new(owner.transform.position.x, owner.transform.position.y));
        }

        public void SendMovementMessage(Vector2 movementVector)
        {
            messageCounter++;
            owner.MovementRequestRPC(movementVector, messageCounter);
            StampLocation();
        }

        public void IsPredictionCorrect(Vector2 NewPosition, ulong messageId)
        {
            if (PreviousLocations.ContainsKey(messageId))
            { 
                if (PreviousLocations[messageId] != NewPosition)
                {
                    Resync(NewPosition, messageId);
                    return;
                } 
            }
            DiscardMessagesBefore(messageId);
        }
        public void DiscardMessagesBefore(ulong messageId)
        {
            // Has bug where some messages arent being deleted
            List<ulong> Remove = new List<ulong>();
            foreach(ulong Id in PreviousLocations.Keys)
            {
                if (Id <= messageId)
                {
                    Remove.Add(messageId);
                }
            }
            foreach (ulong Id in Remove)
            {
                PreviousLocations.Remove(Id);
            }
        }
        public void Resync(Vector2 NewPosition, ulong MessageId)
        {
            Debug.Log(NewPosition + " " + transform.position);
            Vector2 Offset = PreviousLocations[MessageId] - NewPosition;
            Debug.Log("I Have Desynced... Correcting by " + Offset, this.gameObject);
            DiscardMessagesBefore(MessageId);
            List<ulong> keys = new List<ulong>(PreviousLocations.Keys);
            ulong NewestMove = keys[0];
            foreach (ulong key in keys)
            {
                PreviousLocations[key] -= Offset;
                if (NewestMove < key)
                {
                    NewestMove = key;
                }
            }
            // Set possition to corrected one 
            transform.position = PreviousLocations[NewestMove];
        }
    }
}

