using System;
using System.Data.SqlTypes;
using Unity.Netcode;
using UnityEngine;

namespace EntityStatistics
{
    /// <summary>
    /// <c>EntityStats</c> is a superclass that denotes the basic stats all entities that meaningfully exist within combat utilize.
    /// </summary>
    public class EntityStats : NetworkBehaviour
    {
        // Level and general stats
        public NetworkVariable<int> Level = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseLevel = 1;

        // Health Values
        public NetworkVariable<CappedStat> Health = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseHealth = 100;

        // Attack Values
        public NetworkVariable<VariableStat> PhysicalAttack = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int basePhysicalAttack = 10;

        public NetworkVariable<VariableStat> MagicalAttack = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseMagicalAttack = 10;

        // Defense Values
        public NetworkVariable<VariableStat> PhysicalDefense = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int basePhysicalDefense = 10;

        public NetworkVariable<VariableStat> MagicalDefense = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseMagicalDefense = 10;

        // Speed Values
        public NetworkVariable<VariableStat> Speed = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseSpeed = 5;

        // Crit/Luck
        public NetworkVariable<VariableStat> CritRate = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseCritRate = 5;

        // Other Resources


        // Methods
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                InitializeValues();
            }
            else
            {
                // Don't initialize. Already made in server
            }
        }

        public void InitializeValues()
        {
            Level.Value = baseLevel;
            Health.Value = new CappedStat() { BaseValue = baseHealth, CurrentValue = baseHealth, MaxValue = baseHealth };
            PhysicalAttack.Value = new VariableStat() { BaseValue = basePhysicalAttack, CurrentValue = basePhysicalAttack };
            MagicalAttack.Value = new VariableStat() { BaseValue = baseMagicalAttack, CurrentValue = baseMagicalAttack };
            PhysicalDefense.Value = new VariableStat() { BaseValue = basePhysicalDefense, CurrentValue = basePhysicalDefense };
            MagicalDefense.Value = new VariableStat() { BaseValue = baseMagicalDefense, CurrentValue = baseMagicalDefense };
            Speed.Value = new VariableStat() { BaseValue = baseSpeed, CurrentValue = baseSpeed };
            CritRate.Value = new VariableStat() { BaseValue = baseCritRate, CurrentValue = baseCritRate };
        }
    }

    public struct CappedStat : INetworkSerializable
    {
        public int MaxValue;
        public int BaseValue;
        public int CurrentValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref MaxValue);
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }

    public struct FloatCappedStat : INetworkSerializable
    {
        public float MaxValue;
        public float BaseValue;
        public float CurrentValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref MaxValue);
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }

    public struct VariableStat : INetworkSerializable
    {
        public int BaseValue;
        public int CurrentValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }

    public struct FloatVariableStat : INetworkSerializable
    {
        public float BaseValue;
        public float CurrentValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }
}
