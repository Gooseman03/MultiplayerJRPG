using Unity.Netcode;

[GenerateSerializationForGenericParameter(0)]
public struct StoredMessage<T> : INetworkSerializable where T : struct, INetworkSerializable
{
    public uint Id;
    public T message;
    public StoredMessage(StoredMessage<T> ToCopy)
    {
        this.Id = ToCopy.Id;
        this.message = ToCopy.message;
    }
    public StoredMessage(uint Id, T message)
    {
        this.Id = Id;
        this.message = message;
    }
    public void NetworkSerialize<T1>(BufferSerializer<T1> serializer) where T1 : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref message);
    }
}