using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Dependencies have changed to much to keep working for now

//namespace Ladder.PlayerMovement1Helpers
//{
//    public class ClientReconcile1 : MonoBehaviour
//    {
//        private Dictionary<int, Vector2> PreviousMessages = new Dictionary<int, Vector2>();
//        private Dictionary<int, Vector2> PreviousInputs = new Dictionary<int, Vector2>();
//        private PlayerMovement1 owner = null;
//        private void Awake()
//        {
//            owner = GetComponent<PlayerMovement1>();
//        }
//        public void StampLocation(int tick, Vector2 input)
//        {
//            PreviousMessages.Add(tick, new(owner.transform.position.x, owner.transform.position.y));
//            PreviousInputs.Add(tick, input);
//        }
//        public void GetPreviousMessages(out int[] ticks, out float[] xs, out float[] ys)
//        {
//            ticks = PreviousInputs.Keys.ToArray();
//            Vector2[] inputs = PreviousInputs.Values.ToArray();
//            xs = new float[inputs.Length];
//            ys = new float[inputs.Length];
//            for (int i = 0; i < inputs.Length; i++)
//            {
//                xs[i] = inputs[i].x;
//                ys[i] = inputs[i].y;
//            }
//            //owner.clientBacklog = ticks;
//        }
//
//        public void IsPredictionCorrect(int tick, Vector2 NewPosition)
//        {
//            if (PreviousMessages.TryGetValue(tick, out Vector2 message))
//            {
//                DiscardMessagesBefore(tick);
//                if (message != NewPosition)
//                {
//                    Resync(tick, NewPosition);
//                }
//            }
//        }
//
//        public void DiscardMessagesBefore(int messageId)
//        {
//            List<int> Remove = new List<int>();
//            foreach(int Id in PreviousMessages.Keys)
//            {
//                if (Id < messageId)
//                {
//                    Remove.Add(Id);
//                }
//            }
//            foreach (int Id in Remove)
//            {
//                PreviousMessages.Remove(Id);
//                PreviousInputs.Remove(Id);
//            }
//        }
//
//        public void Resync(int tick, Vector2 NewPosition)
//        {
//            Debug.Log(NewPosition + " " + PreviousMessages[tick]);
//            Vector2 Offset = PreviousMessages[tick] - NewPosition;
//            Debug.Log("I Have Desynced at MessageID" + tick + "... Correcting by " + Offset, this.gameObject);
//            List<int> keys = new List<int>(PreviousMessages.Keys);
//            foreach (int key in keys)
//            {
//                PreviousMessages[key] -= Offset;
//            }
//            // Set position to corrected one 
//            transform.position -= new Vector3(Offset.x, Offset.y, 0);
//        }
//    }
//}

