public interface IQueueUser<T> where T : struct
{
    public abstract bool CheckForDesync(T message, T newMessage);
    public abstract T Resync(uint messageId, T message);
    public abstract void OnMessageNotStored(uint messageId, T message);
    public abstract T ApplyOffset(uint offset, T message);
}