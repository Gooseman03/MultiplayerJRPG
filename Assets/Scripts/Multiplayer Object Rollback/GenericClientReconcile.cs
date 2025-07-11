using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GenericClientReconcile<T> where T : struct, INetworkSerializable
{
    public IQueueUser<T> Creator;
    public readonly Queue<T> queue = new Queue<T>(); // Where you were in the past sorted by the tick
    // Takes in the current Tick and what the Inputs were this tick and stores them for reconciliation with the server
    public void RecordGameState(uint time, T message)
    {
        queue.Add(new StoredMessage<T>() { Id = time, message = message });
    }

    public bool CheckForInputAt(uint index)
    {
        if (queue.TryGetValue(index, out T message))
        {
            return true;
        }
        return false;
    }

    // Supply a Index and how many messages you want 
    // It will return a array of previousInputs starting at the Index back
    public StoredMessage<T>[] GrabRedundantMessages(uint StartIndex, int count)
    {
        List<StoredMessage<T>> result = new();
        for (uint i = 0; i < count; i++)
        {
            if (queue.TryGetValue(StartIndex - i, out T message))
            {
                result.Add(new StoredMessage<T>(StartIndex - i, message));
            }
        }
        return result.ToArray();
    }

    // Checks whether the predicted position for a given message ID matches the actual position.
    // If there's a mismatch, it triggers a resynchronization.
    public void IsPredictionCorrect(T messageFromServer, uint messageId)
    {
        if (queue.TryGetValue(messageId, out T storedPosition))
        {
            //DiscardBefore(messageId);
            if (Creator.CheckForDesync(storedPosition, messageFromServer))
            {
                T offset = Creator.Resync(storedPosition, messageFromServer);
                Debug.Log("A Object Desynced at MessageID " + messageId + "... Correcting by " + offset);
                // Adjust all stored predicted positions to realign with the actual server-corrected position
                List<uint> keys = new(queue.Keys);
                foreach (uint key in keys)
                {
                    if (key >= messageId)
                        Creator.ApplyOffset(key, offset);
                }

            }
        }
        else
        {
            Creator.OnMessageNotStored(messageId, messageFromServer);
        }
    }
    // This will discard all stored inputs and locations from the index Suppied Back
    // Use it for removing old messages after the server gives the correct one
    public void DiscardBefore(uint messageId)
    {
        var keysToRemove = queue.Keys.Where(id => id < messageId).ToList();
        foreach (var key in keysToRemove)
        {
            queue.Remove(key);
        }
    }
}
