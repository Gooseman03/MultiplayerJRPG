using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.PlayerMovementHelpers
{
    public class ClientReconcile : NetworkBehaviour
    {
        private Dictionary<uint, Vector2> PreviousLocations = new Dictionary<uint, Vector2>(); // Where you were in the past sorted by the tick
        private Dictionary<uint, Inputs> PreviousMessages = new Dictionary<uint, Inputs>(); // What Inputs were sent to the server on the tick
        private PlayerMovement owner = null; // Reference to the PlayerMovement System for reconciliation 

        // Finds the playerMovement Script
        private void Awake()
        {
            owner = GetComponent<PlayerMovement>();
        }

        /* 
         * Takes in the current Tick and what the Inputs were this tick and stores them for reconciliation with the server
         */
        public void RecordGameState(uint time,Inputs message)
        {
            PreviousMessages.Add(time, message);
            PreviousLocations.Add(time, new(transform.position.x, transform.position.y));
        }

        public bool DoesPreviousInputExistAt(uint index)
        {
            if (PreviousMessages.TryGetValue(index, out Inputs message))
            {
                return true;
            }
            return false;
        }
        /* 
         * Supply a Index and how many messages you want 
         * It will return a array of previousInputs starting at the Index back
         */
        public MessageBundle[] GrabPreviousInputs(uint StartIndex, int count)
        {
            List<MessageBundle> result = new ();
            for (uint i = 0; i < count; i++)
            {
                if (PreviousMessages.TryGetValue(StartIndex - i, out Inputs message))
                {
                    result.Add(new(StartIndex - i, message));
                }
            }
            return result.ToArray();
        }

        /*
         * Checks whether the predicted position for a given message ID matches the actual position.
         * If there's a mismatch, it triggers a resynchronization.
        */
        public void IsPredictionCorrect(Vector2 NewPosition, uint messageId)
        {
            if (PreviousLocations.TryGetValue(messageId, out Vector2 message))
            {
                DiscardBefore(messageId);
                if (message != NewPosition)
                {
                    Resync(NewPosition, messageId);
                }
            }
        }

        /* 
         * This will discard all stored inputs and locations from the index Suppied Back
         * Use it for removing old messages after the server gives the correct one
         */
        public void DiscardBefore(uint messageId)
        {
            List<uint> Remove = new List<uint>();
            foreach (uint Id in PreviousLocations.Keys)
            {
                if (Id < messageId)
                {
                    Remove.Add(Id);
                }
            }
            foreach (uint Id in Remove)
            {
                PreviousLocations.Remove(Id);
                PreviousMessages.Remove(Id);
            }
        }

        // For Now just calls Resync Position Should be used in future to call all reconciliation functions
        public void Resync(Vector2 NewPosition, uint MessageId)
        {
            ResyncPosition(NewPosition,MessageId);
        }

        // Resynchronizes the player's position when a mismatch between predicted and actual position is detected.
        public void ResyncPosition(Vector2 NewPosition, uint MessageId)
        {
            // Calculate the offset between the predicted and actual position
            Vector2 Offset = PreviousLocations[MessageId] - NewPosition;

            // Log that a desync occurred and how it will be corrected
            Debug.Log("I Have Desynced at MessageID " + MessageId + "... Correcting Movement by " + Offset, this.gameObject);

            // Adjust all stored predicted positions to realign with the actual server-corrected position
            List<uint> keys = new List<uint>(PreviousLocations.Keys);
            foreach (uint key in keys)
            {
                PreviousLocations[key] -= Offset;
            }

            // Apply the offset correction to the actual player GameObject's transform
            transform.position -= new Vector3(Offset.x, Offset.y, 0);
        }
    }
}

