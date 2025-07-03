using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Ladder.Multiplayer.Multiplayer.Syncing
{
    public class Multiplayer : NetworkBehaviour
    {
        public UnityEvent AtTriggerTime;
        // Number of ticks to delay the event after the system starts the timer.
        public int TicksToDelay = 30;
        // Property to convert delay from ticks to seconds. It calculates the time delay based on the local tick rate.
        public float SecondsToDelay
        {
            get
            {
                float TicksToDelayInSeconds = TicksToDelay / NetworkManager.LocalTime.FixedDeltaTime;
                return TicksToDelayInSeconds;
            }
            set
            {
                // Convert from seconds to ticks.
                float TicksToDelayInSeconds = value;
                TicksToDelay = Mathf.RoundToInt(TicksToDelayInSeconds * NetworkManager.LocalTime.TickRate);
            }
        }

        // Method to invoke the event when trigger time occurs.
        public void OnTriggerTime()
        {
            AtTriggerTime.Invoke();
        }

        /// <summary>
        /// Starts the timer on the server and syncs it with the clients.
        /// Make sure to set SecondsToDelay to the duration before the event is triggered.
        /// <para>Example usage: Use this to sync an event (e.g., door opening) across clients after a delay.</para>
        /// </summary>
        public bool StartTimer()
        {
            int triggerTick = NetworkManager.LocalTime.Tick + TicksToDelay;
            if (!IsServer)
            {
                Debug.LogError("Cannot call a Server-side method on a client");
                return false;
            }
            else
            {
                StartCoroutine(WaitForTrigger(triggerTick));
                NotifyClientsOfBeginningTickRpc(triggerTick);
                return true;
            }
        }

        // This method is called on the client to prepare for the event when the server tells it when to trigger.
        private void StartWaitingForTickClient(int whenToBeginAsTick)
        {
            int currentTick = NetworkManager.LocalTime.Tick;
            if (currentTick < whenToBeginAsTick)
            {
                Debug.Log("Waiting...");
                StartCoroutine(WaitForTrigger(whenToBeginAsTick));
            }
            else
            {
                // If the event is overdue, trigger it immediately.
                Debug.Log("Message was Late... Triggering Now");
                OnTriggerTime();
            }
        }

        // Coroutine that waits for the specific tick before triggering the event.
        private IEnumerator WaitForTrigger(int triggerTick)
        {
            // To find how long to wait. Find how long till the trigger tick. Then divide by how many ticks there are per second
            float timeToWait = (triggerTick - (float)NetworkManager.LocalTime.TickWithPartial) / NetworkManager.LocalTime.TickRate;
            yield return new WaitForSeconds(timeToWait);
            OnTriggerTime();
        }

        [Rpc(target: SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        private void NotifyClientsOfBeginningTickRpc(int triggerAtTick)
        {
            StartWaitingForTickClient(triggerAtTick);
        }
    }
}