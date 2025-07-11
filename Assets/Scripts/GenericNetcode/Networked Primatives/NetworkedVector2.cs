using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
[GenerateSerializationForGenericParameter(0)]
public struct NetworkedVector2 : INetworkSerializable
{
    public Vector2 Value;
    public NetworkedVector2(Vector2 value)
    {
        this.Value = value;
    }

    public float x => this.Value.x;
    public float y => this.Value.y;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator String(NetworkedVector2 v) => v.ToString();
    public static implicit operator Vector2(NetworkedVector2 v) => v.Value;
    public static implicit operator Vector3(NetworkedVector2 v) => (Vector3)v.Value;
    public static implicit operator NetworkedVector2(Vector3 v) => new NetworkedVector2(v);
    public static implicit operator NetworkedVector2(Vector2 v) => new NetworkedVector2(v);

    public static NetworkedVector2 operator +(NetworkedVector2 value1, NetworkedVector2 value2) => value1.Value + value2.Value;
    public static NetworkedVector2 operator -(NetworkedVector2 value1, NetworkedVector2 value2) => value1.Value - value2.Value;
    public static NetworkedVector2 operator *(NetworkedVector2 value1, int value2) => value1.Value * value2;
    public static NetworkedVector2 operator *(int value1, NetworkedVector2 value2) => value1 * value2.Value;
}

