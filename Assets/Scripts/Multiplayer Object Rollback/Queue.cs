using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class Queue<T> where T : struct, INetworkSerializable
{
    public Dictionary<uint, T> messageBuffer = new Dictionary<uint, T>();

    public IEnumerable<uint> Keys => messageBuffer.Keys;
    public List<T> Values => messageBuffer.Values.ToList();

    public T this[uint key]
    {
        get => messageBuffer[key];
        set => messageBuffer[key] = value;
    }
    public Queue(uint id, T inputsInit)
    {
        messageBuffer[id] = inputsInit;
        var a = messageBuffer.Values;
    }
    public Queue() { }
    // Forces the message into the buffer overwriting previously held Key
    public void Add(StoredMessage<T> message)
    {
        messageBuffer[message.Id] = message.message;
    }
    //Takes in a index and outputs the message if it exists in the buffer.
    //If it cant find the index it will return false. 
    public bool TryGetValue(uint messageId, out T inputs)
    {
        inputs = new();
        if (messageBuffer.TryGetValue(messageId, out T foundInputs))
        {
            inputs = foundInputs;
            return true;
        }
        return false;
    }
    //Takes in the current Tick and attempts to add more messages to the buffer
    //If the message is already in the buffer or if the server is past the intended tick for it will be dicarded
    public void TryAddValue(uint time, StoredMessage<T> message)
    {
        if (message.Id <= time) { return; }
        messageBuffer.TryAdd(message.Id, message.message);
    }
    // Will remove the message from the buffer at the index supplied
    public void Remove(uint key) => messageBuffer.Remove(key);
    public bool ContainsKey(uint key) => messageBuffer.ContainsKey(key);
}