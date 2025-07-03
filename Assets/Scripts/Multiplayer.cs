using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Ladder.Multiplayer.Multiplayer.Syncing
{
    public delegate void AtTriggerTimeEvent();
    public class Multiplayer : NetworkBehaviour
    {
        public event AtTriggerTimeEvent AtTriggerTime;

        [SerializeField] private bool shouldDebugEvents = false;
        [SerializeField] private bool willSendDebugTime = true;
        public int TicksToDelay = 5;
        public float SecondsToDelay
        {
            get
            {
                float TicksToDelayInSeconds = TicksToDelay / NetworkManager.LocalTime.FixedDeltaTime;
                return TicksToDelayInSeconds;
            }
            set
            {
                float TicksToDelayInSeconds = value;
                TicksToDelay = Mathf.RoundToInt( TicksToDelayInSeconds * NetworkManager.LocalTime.TickRate );
            }
        }

        private void OnTriggerTime()
        {
            AtTriggerTime();
        }

        private void Update()
        {
            if (IsServer)
            {
                if(willSendDebugTime)
                {
                    willSendDebugTime = false;
                    NotifyClientsOfBeginningTickRpc(NetworkManager.LocalTime.Tick + TicksToDelay);
                }
            }
            if (shouldDebugEvents)
            {
                OnTriggerTime();
            }
        }

        /// <summary>
        /// <para>Call this when the requirements for your system are met.</para>
        /// <para>Make sure to set SecondsToDelay to the duration before the door opens</para>
        /// <example>
        /// <para>If you need to open a door when a puzzle is completed. Then call this when the puzzle is completed. It will sync all of the doors opening.</para>
        /// </example>
        /// </summary>
        public void StartTimer()
        {
            if (!IsServer)
            {
                throw new System.Exception("Calling a Server-side method on a client");
            }
            int triggerTick = NetworkManager.LocalTime.Tick + TicksToDelay;
            if (IsServer)
            {
                StartCoroutine(WaitForTrigger(triggerTick));
                NotifyClientsOfBeginningTickRpc(triggerTick);
            }
        }

        private void StartWaitingForTickClient(int whenToBeginAsTick)
        {
            int currentTick = NetworkManager.LocalTime.Tick;
            if (currentTick < whenToBeginAsTick)
            {
                StartCoroutine(WaitForTrigger(whenToBeginAsTick));
                Debug.Log("Waiting...");
            }
            else
            {
                Debug.Log("Message was Late... Triggering Now");
            }
        }

        private IEnumerator WaitForTrigger(int triggerTick)
        {
            // To find how long to wait. Find how long till the trigger tick. Then divide by how many ticks there are per second
            float timeToWait = (triggerTick - (float)NetworkManager.LocalTime.TickWithPartial) / NetworkManager.LocalTime.TickRate;
            yield return new WaitForSeconds(timeToWait);
            OnTriggerTime();
        }

        [Rpc(target:SendTo.NotServer ,Delivery = RpcDelivery.Reliable)]
        private void NotifyClientsOfBeginningTickRpc(int triggerAtTick) 
        {
            StartWaitingForTickClient(triggerAtTick);
        }
    }
}

