using Unity.Netcode;

public interface IQueueUser<T> where T : struct
{
    public abstract bool CheckForDesync(T message, T newMessage);
    public abstract T Resync(T message1, T message2);
    public abstract void OnMessageNotStored(uint messageId, T message);
    public void ApplyOffset(uint key, T offset);
}