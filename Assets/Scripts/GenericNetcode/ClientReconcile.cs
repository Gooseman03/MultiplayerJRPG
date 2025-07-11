using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.PlayerMovementHelpers
{
    public class ClientReconcile : NetworkBehaviour
    {
        private Dictionary<uint, Vector2> previousLocations = new Dictionary<uint, Vector2>(); // Where you were in the past sorted by the tick
        private Dictionary<uint, Inputs> previousMessages = new Dictionary<uint, Inputs>(); // What Inputs were sent to the server on the tick

        // Takes in the current Tick and what the Inputs were this tick and stores them for reconciliation with the server
        public void RecordGameState(uint time, Inputs message)
        {
            previousMessages.Add(time, message);
            previousLocations.Add(time, new(transform.position.x, transform.position.y));
        }

        public bool CheckForInputAt(uint index)
        {
            if (previousMessages.TryGetValue(index, out Inputs message))
            {
                return true;
            }
            return false;
        }
        /* 
         * Supply a Index and how many messages you want 
         * It will return a array of previousInputs starting at the Index back
         */
        public StoredMessage<Inputs>[] GrabRedundantMessages(uint StartIndex, int count)
        {
            List<StoredMessage<Inputs>> result = new();
            for (uint i = 0; i < count; i++)
            {
                if (previousMessages.TryGetValue(StartIndex - i, out Inputs message))
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
        public void IsPredictionCorrect(Inputs newInputs, uint messageId)
        {
            if (previousLocations.TryGetValue(messageId, out Vector2 message))
            {
                DiscardBefore(messageId);
                if (message != newInputs.Position)
                {
                    Resync(newInputs.Position, messageId);
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
            foreach (uint Id in previousLocations.Keys)
            {
                if (Id < messageId)
                {
                    Remove.Add(Id);
                }
            }
            foreach (uint Id in Remove)
            {
                previousLocations.Remove(Id);
                previousMessages.Remove(Id);
            }
        }

        // For Now just calls Resync Position Should be used in future to call all reconciliation functions
        public void Resync(Vector2 NewPosition, uint MessageId)
        {
            ResyncPosition(NewPosition, MessageId);
        }

        // Resynchronizes the player's position when a mismatch between predicted and actual position is detected.
        public void ResyncPosition(Vector2 NewPosition, uint MessageId)
        {
            // Calculate the offset between the predicted and actual position
            Vector2 Offset = previousLocations[MessageId] - NewPosition;

            // Log that a desync occurred and how it will be corrected
            Debug.Log("I Have Desynced at MessageID " + MessageId + "... Correcting Movement by " + Offset, this.gameObject);

            // Adjust all stored predicted positions to realign with the actual server-corrected position
            List<uint> keys = new List<uint>(previousLocations.Keys);
            foreach (uint key in keys)
            {
                previousLocations[key] -= Offset;
            }

            // Apply the offset correction to the actual player GameObject's transform
            transform.position -= new Vector3(Offset.x, Offset.y, 0);
        }
    }
}

