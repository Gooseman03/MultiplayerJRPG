using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.PlayerMovementHelpers
{
    public class ClientReconcile : NetworkBehaviour
    {
        private Dictionary<ulong, Vector2> PreviousMessages = new Dictionary<ulong, Vector2>();
        private PlayerMovement owner = null;
        private ulong messageCounter = 0;
        private void Awake()
        {
            owner = GetComponent<PlayerMovement>();
        }
        public void StampLocation()
        {
            PreviousMessages.Add(messageCounter, new(owner.transform.position.x, owner.transform.position.y));
        }

        public void SendMovementMessage(Vector2 movementVector)
        {
            //if (CorrectionMessages != 0)
            //{
            //    List<MessageBundle> messages = new List<MessageBundle>();
            //    for (int i = CorrectionMessages; i > 0; i--)
            //    {
            //        if (PreviousMessages.TryGetValue(messageCounter - (ulong)i, out Vector2 location))
            //        {
            //            messages.Add(new MessageBundle() { Input = location, Id = messageCounter - (ulong)i });
            //        }
            //    }
            //    Vector2[] vectors = new Vector2[messages.Count + 1];
            //    ulong[] ids = new ulong[messages.Count + 1];
            //    int c = 0;
            //    foreach (MessageBundle message in messages)
            //    {
            //        vectors[c] = message.Input;
            //        ids[c] = message.Id;
            //        c++;
            //    }
            //    messageCounter++;
            //    vectors[c] = movementVector;
            //    ids[c] = messageCounter;
            //    owner.MovementRequestRPC(vectors, ids);
            //    StampLocation();
            //    return;
            //}
            messageCounter++;
            owner.MovementRequestRPC(movementVector, messageCounter);
            StampLocation();
        }

        public void IsPredictionCorrect(Vector2 NewPosition, ulong messageId)
        {
            if (PreviousMessages.TryGetValue(messageId, out Vector2 message))
            {
                DiscardMessagesBefore(messageId);
                if (message != NewPosition)
                {
                    Resync(NewPosition, messageId);
                }
            }
        }

        public void DiscardMessagesBefore(ulong messageId)
        {
            List<ulong> Remove = new List<ulong>();
            foreach(ulong Id in PreviousMessages.Keys)
            {
                if (Id < messageId)
                {
                    Remove.Add(Id);
                }
            }
            foreach (ulong Id in Remove)
            {
                PreviousMessages.Remove(Id);
            }
        }
        public void Resync(Vector2 NewPosition, ulong MessageId)
        {
            Debug.Log(NewPosition + " " + PreviousMessages[MessageId]);
            Vector2 Offset = PreviousMessages[MessageId] - NewPosition;
            Debug.Log("I Have Desynced at MessageID" + MessageId + "... Correcting by " + Offset, this.gameObject);
            List<ulong> keys = new List<ulong>(PreviousMessages.Keys);
            foreach (ulong key in keys)
            {
                PreviousMessages[key] -= Offset;
            }
            // Set position to corrected one 
            transform.position -= new Vector3(Offset.x, Offset.y, 0);
        }
    }
}

