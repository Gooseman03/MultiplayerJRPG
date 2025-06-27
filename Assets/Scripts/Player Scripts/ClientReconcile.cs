using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.PlayerMovementHelpers
{
    public class ClientReconcile : NetworkBehaviour
    {
        private Dictionary<uint, Vector2> PreviousMessages = new Dictionary<uint, Vector2>();
        private Dictionary<uint, Vector2> PreviousInputs = new Dictionary<uint, Vector2>();
        private PlayerMovement owner = null;
        private void Awake()
        {
            owner = GetComponent<PlayerMovement>();
        }
        public void StampLocation(uint time, Vector2 input)
        {
            PreviousInputs.Add(time, input);
            PreviousMessages.Add(time, new(owner.transform.position.x, owner.transform.position.y));
        }
        public Dictionary<uint, Vector2> GrabPreviousInputs(int count, uint StartIndex)
        {
            Dictionary<uint, Vector2> result = new Dictionary<uint, Vector2>();
            for (uint i = 0; i < count; i++)
            {
                if (PreviousInputs.TryGetValue(StartIndex - i, out Vector2 vector))
                {
                    result.Add(StartIndex - i, vector);
                }
            }
            return result;
        }
        public void IsPredictionCorrect(Vector2 NewPosition, uint messageId)
        {
            if (PreviousMessages.TryGetValue(messageId, out Vector2 message))
            {
                DiscardBefore(messageId);
                if (message != NewPosition)
                {
                    Resync(NewPosition, messageId);
                }
            }
        }
        public void DiscardBefore(uint messageId)
        {
            List<uint> Remove = new List<uint>();
            foreach (uint Id in PreviousMessages.Keys)
            {
                if (Id < messageId)
                {
                    Remove.Add(Id);
                }
            }
            foreach (uint Id in Remove)
            {
                PreviousMessages.Remove(Id);
                PreviousInputs.Remove(Id);
            }
        }
        public void Resync(Vector2 NewPosition, uint MessageId)
        {
            Debug.Log(NewPosition + " " + PreviousMessages[MessageId]);
            Vector2 Offset = PreviousMessages[MessageId] - NewPosition;
            Debug.Log("I Have Desynced at MessageID " + MessageId + "... Correcting by " + Offset, this.gameObject);
            List<uint> keys = new List<uint>(PreviousMessages.Keys);
            foreach (uint key in keys)
            {
                PreviousMessages[key] -= Offset;
            }
            // Set position to corrected one 
            transform.position -= new Vector3(Offset.x, Offset.y, 0);
        }
    }
}

