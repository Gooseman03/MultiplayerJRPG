using Unity.Netcode;
using UnityEngine.Tilemaps;

namespace Ladder.Netcode.StatTypes
{
    /*public struct CappedStat<T> : INetworkSerializable
        where T : struct, INetworkSerializable
    {
        public T BaseValue;
        public T CurrentValue;
        public T MaxValue;
        public T MinValue;

        public void NetworkSerialize<T1>(BufferSerializer<T1> serializer) where T1 : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
            serializer.SerializeValue(ref MaxValue);
            serializer.SerializeValue(ref MinValue);
        }
    }

    public struct VariableStat<T> : INetworkSerializable
        where T : struct, INetworkSerializable
    {
        public T BaseValue;
        public T CurrentValue;

        public void NetworkSerialize<T1>(BufferSerializer<T1> serializer) where T1 : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }*/

    public class CappedStat<T> : VariableStat<T>
    {
        public T MaxValue;
        public T MinValue;

        public CappedStat(T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm)
            : base(value, readPerm, writePerm)
        {
            MaxValue = value;
            MinValue = value;
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            // Unneeded... Maybe?
        }

        public override void ReadField(FastBufferReader reader)
        {
            base.ReadField(reader);
            T newMax = default;
            T newMin = default;
            NetworkVariableSerialization<T>.Read(reader, ref newMax);
            NetworkVariableSerialization<T>.Read(reader, ref newMin);
            MaxValue = newMax;
            MinValue = newMin;
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            // Unneeded... Maybe?
        }

        public override void WriteField(FastBufferWriter writer)
        {
            base.WriteField(writer);
            NetworkVariableSerialization<T>.Write(writer, ref MaxValue);
            NetworkVariableSerialization<T>.Write(writer, ref MinValue);
        }
    }

    [GenerateSerializationForGenericParameter(0)]
    public class VariableStat<T> : NetworkVariableBase
    {
        public T BaseValue;
        public T CurrentValue;

        public VariableStat(T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm)
            : base(readPerm, writePerm)
        {
            BaseValue = value;
            CurrentValue = value;
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            // Unneeded... Maybe?
        }

        public override void ReadField(FastBufferReader reader)
        {
            T newBase = default;
            T newCurrent = default;
            NetworkVariableSerialization<T>.Read(reader, ref newBase);
            NetworkVariableSerialization<T>.Read(reader, ref newCurrent);
            BaseValue = newBase;
            CurrentValue = newCurrent;
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            // Unneeded... Maybe?
        }

        public override void WriteField(FastBufferWriter writer)
        {
            NetworkVariableSerialization<T>.Write(writer, ref BaseValue);
            NetworkVariableSerialization<T>.Write(writer, ref CurrentValue);
        }
    }
}

namespace Ladder.Netcode.Primitives
{
    public struct NetworkedInt : INetworkSerializable
    {
        public int Value;

        public NetworkedInt(int value) => Value = value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Value);
        }

        public static implicit operator NetworkedInt(int v) => new NetworkedInt(v);
        public static implicit operator int(NetworkedInt v) => v.Value;
        public static NetworkedInt operator +(NetworkedInt v1, NetworkedInt v2) => v1.Value + v2.Value;
        public static NetworkedInt operator -(NetworkedInt v1, NetworkedInt v2) => v1.Value - v2.Value;
        public static NetworkedInt operator *(NetworkedInt v1, NetworkedInt v2) => v1.Value * v2.Value;
        public static NetworkedInt operator /(NetworkedInt v1, NetworkedInt v2) => v1.Value / v2.Value;
        public static NetworkedInt operator %(NetworkedInt v1, NetworkedInt v2) => v1.Value % v2.Value;
    }

    public struct NetworkedFloat : INetworkSerializable
    {
        public float Value;

        public NetworkedFloat(float value) => Value = value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Value);
        }

        public static implicit operator NetworkedFloat(float v) => new NetworkedFloat(v);
        public static implicit operator float(NetworkedFloat v) => v.Value;
        public static NetworkedFloat operator +(NetworkedFloat v1, NetworkedFloat v2) => v1.Value + v2.Value;
        public static NetworkedFloat operator -(NetworkedFloat v1, NetworkedFloat v2) => v1.Value - v2.Value;
        public static NetworkedFloat operator *(NetworkedFloat v1, NetworkedFloat v2) => v1.Value * v2.Value;
        public static NetworkedFloat operator /(NetworkedFloat v1, NetworkedFloat v2) => v1.Value / v2.Value;
        public static NetworkedFloat operator %(NetworkedFloat v1, NetworkedFloat v2) => v1.Value % v2.Value;
    }
}