using System;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.EntityStatistics
{

    // A list of the damage types that can be used in game.
    public enum DamageType
    {
        physical,
        magical,
        trueDamage
    }

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
        public NetworkVariable<FloatVariableStat> CritRate = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const float baseCritRate = 0.05f;

        public NetworkVariable<FloatVariableStat> CritDamage = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const float baseCritDamage = 1.2f;

        // Other Resources

        // Methods
        // Setup for object once it spawns on the network.
        public override void OnNetworkSpawn() // Still missing OnValueChanged event setup.
        {
            if (IsServer)
            {
                // Collect information from files
                InitializeValues();
            }
            else
            {
                // Don't initialize. Already made in server
                SetupOnChangedEvents();
            }
        }

        // Disolve all unneeded values before removing object.
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {

            }
            else
            {

            }
        }

        // Initializes all values to default values.
        public virtual void InitializeValues()
        {
            Level.Value = baseLevel;
            Health.Value = new CappedStat() 
            {
                BaseValue = baseHealth, CurrentValue = baseHealth, MaxValue = baseHealth, MinValue = 0
            };
            PhysicalAttack.Value = new VariableStat()
            {
                BaseValue = basePhysicalAttack, CurrentValue = basePhysicalAttack
            };
            MagicalAttack.Value = new VariableStat()
            {
                BaseValue = baseMagicalAttack, CurrentValue = baseMagicalAttack
            };
            PhysicalDefense.Value = new VariableStat()
            {
                BaseValue = basePhysicalDefense, CurrentValue = basePhysicalDefense
            };
            MagicalDefense.Value = new VariableStat()
            {
                BaseValue = baseMagicalDefense, CurrentValue = baseMagicalDefense
            };
            Speed.Value = new VariableStat()
            {
                BaseValue = baseSpeed, CurrentValue = baseSpeed
            };
            CritRate.Value = new FloatVariableStat()
            {
                BaseValue = baseCritRate, CurrentValue = baseCritRate
            };
            CritDamage.Value = new FloatVariableStat()
            {
                BaseValue = baseCritDamage, CurrentValue = baseCritDamage
            };
        }

        // Attaches all events to proper channels.
        public virtual void SetupOnChangedEvents()
        {
            return;
        }

        // Level Methods


        // Damage Methods
        // Does damage to owner of account.
        public void DoDamage(DamageType damageType, int damage)
        {
            int reducedDamage;
            switch (damageType)
            {
                case DamageType.physical:
                    reducedDamage = ReduceDamage(damage, PhysicalDefense.Value.CurrentValue);
                    break;

                case DamageType.magical:
                    reducedDamage = ReduceDamage(damage, MagicalDefense.Value.CurrentValue);
                    break;

                case DamageType.trueDamage:
                    reducedDamage = damage;
                    break;

                default:
                    throw new ArgumentException("How the fuck did we get here? Your defense damage type is stupid.");
            }
            TakeDamage(reducedDamage);
        }

        // Currently implemented as flat damage reduction, can update later to be more complex if need be
        // Calculates the reduced damage based on the reduction value applied.
        private int ReduceDamage(int damage, int reductionValue)
        {
            return Math.Max(damage - reductionValue, 0);
        }

        // Alters the damage of the owner.
        private void TakeDamage(int damage)
        {
            int temp = Health.Value.CurrentValue - damage;
            if (temp <= Health.Value.MinValue)
            {
                Health.Value = new(Health.Value, 0);
                Die();
            }
            else
            {
                Health.Value = new(Health.Value, temp);
            }
        }

        // Activates the death sequence for the owner.
        public void Die()
        {
            Debug.Log("HOLY SHIT I'M DEAD");
        }

        // Heals owner by set amount.
        public void Heal(int healAmount)
        {
            if (healAmount <= 0)
            {
                return;
            }
            int temp = Math.Min(Health.Value.CurrentValue + healAmount, Health.Value.MaxValue);
            Health.Value = new(Health.Value, temp);
        }

        // Alters the maximum health by specified amount. Scales owner's current health if specified by boolean.
        public void AlterMaxHealth(int alteration, bool shouldAlterCurrent)
        {
            int temp = Health.Value.MaxValue + alteration;
            if (temp <= Health.Value.MinValue)
            {
                Health.Value = new()
                {
                    CurrentValue = Health.Value.MinValue,
                    BaseValue = Health.Value.BaseValue,
                    MaxValue = Health.Value.MinValue,
                    MinValue = Health.Value.MinValue,
                };
                Die();
            }
            else if (shouldAlterCurrent)
            {
                TakeDamage(alteration); // Simple automatic health alteration system. Scales health by amount changed.
            }
        }

        // Resets the maximum health to the stored base value. Sets current health to maximum if specified by boolean or is greater than base health.
        public void ResetMaxHealth(bool resetCurrentHealth)
        {
            if (resetCurrentHealth || Health.Value.CurrentValue > Health.Value.BaseValue)
            {
                Health.Value = new()
                {
                    CurrentValue = Health.Value.BaseValue,
                    BaseValue = Health.Value.BaseValue,
                    MaxValue = Health.Value.BaseValue,
                    MinValue = Health.Value.MinValue
                };
            }
            else
            {
                Health.Value = new()
                {
                    CurrentValue = Health.Value.CurrentValue,
                    BaseValue = Health.Value.BaseValue,
                    MaxValue = Health.Value.BaseValue,
                    MinValue = Health.Value.MinValue
                };
            }
        }

        
        // Attack Methods
        // Calculates the damage modified after the relevant damage types are implemented.
        public int CalculateDamage(DamageType damageType, int baseAttackDamage)
        {
            int finalDamage;
            switch (damageType)
            {
                case DamageType.physical:
                    finalDamage = CalculateBonusDamage(baseAttackDamage, PhysicalAttack.Value.CurrentValue);
                    break;

                case DamageType.magical:
                    finalDamage = CalculateBonusDamage(baseAttackDamage, MagicalAttack.Value.CurrentValue);
                    break;

                case DamageType.trueDamage:
                    finalDamage = baseAttackDamage;
                    break;

                default:
                    throw new ArgumentException("How the hell did we get here? Your attack damage type is stupid.");
            }
            return finalDamage;
        }

        // Calculates the bonus damage gathered by crit and modifiers.
        private int CalculateBonusDamage(int baseAttackDamage, int modifier)
        {
            //bool isCrit = ; Do randomization
            return Math.Max(Mathf.RoundToInt((1f + modifier) * baseAttackDamage), 0);
        }

        // Alters the attack stat of the specified damage type.
        public void AlterAttackOfType(DamageType damageType, int alteration)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalAttack.Value = new(PhysicalAttack.Value, PhysicalAttack.Value.CurrentValue + alteration);
                    break;

                case DamageType.magical:
                    MagicalAttack.Value = new(MagicalAttack.Value, MagicalAttack.Value.CurrentValue + alteration);
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify the amount you are altering the attack of.");
            }
        }

        // Resets the attack modifier of the specified damage type.
        public void ResetAttackOfType(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalAttack.Value = new(PhysicalAttack.Value, PhysicalAttack.Value.BaseValue);
                    break;
                
                case DamageType.magical:
                    MagicalAttack.Value = new(MagicalAttack.Value, MagicalAttack.Value.BaseValue);
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify attack damage type to reset.");
            }
        }

        // Alters the crit rate by the specified alteration.
        public void AlterCritRate(float alteration)
        {
            CritRate.Value = new(CritRate.Value, Math.Max(CritRate.Value.CurrentValue + alteration, 0f));
        }

        // Resets the crit rate back to the specified base value.
        public void ResetCritRate()
        {
            CritRate.Value = new(CritRate.Value, CritRate.Value.BaseValue);
        }

        // Alters the crit damage by the specified amount.
        public void AlterCritDamage(float alteration)
        {
            CritDamage.Value = new(CritDamage.Value, Math.Max(CritDamage.Value.CurrentValue + alteration, 0f));
        }

        // Resets the crit damage to it's specified default value.
        public void ResetCritDamage()
        {
            CritDamage.Value = new(CritDamage.Value, CritDamage.Value.CurrentValue);
        }


        // Defense Methods
        // Alters the defense of the specified damage type by the specified amount.
        public void AlterDefenseOfType(DamageType damageType, int alteration)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalDefense.Value = new(PhysicalDefense.Value, PhysicalDefense.Value.CurrentValue + alteration);
                    break;

                case DamageType.magical:
                    MagicalDefense.Value = new(MagicalDefense.Value, MagicalDefense.Value.CurrentValue + alteration);
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify the amount you are altering the defense of.");
            }
        }

        // Resets the defense of the specified damage type to its specified base value.
        public void ResetDefenseOfType(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalDefense.Value = new(PhysicalDefense.Value, PhysicalDefense.Value.BaseValue);
                    break;

                case DamageType.magical:
                    MagicalDefense.Value = new(MagicalDefense.Value, MagicalDefense.Value.BaseValue);
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify defense type to reset.");
            }
        }


        // Speed Methods
        // Alters the speed by the specified amount.
        public void AlterSpeed(int alteration)
        {
            Speed.Value = new(Speed.Value, Speed.Value.CurrentValue + alteration);
        }

        // Resets the speed to the stored base value.
        public void ResetSpeed()
        {
            Speed.Value = new(Speed.Value, Speed.Value.BaseValue);
        }
    }

    /// <summary>
    /// <c>CappedStat</c> represents a stat an entity involved in combat has that includes some maximum and minimum value.
    /// </summary>
    public struct CappedStat : INetworkSerializable
    {
        public int BaseValue;
        public int CurrentValue;
        public int MaxValue;
        public int MinValue;

        public CappedStat(CappedStat toCopy, int newCurrent)
        {
            BaseValue = toCopy.BaseValue;
            CurrentValue = newCurrent;
            MaxValue = toCopy.MaxValue;
            MinValue = toCopy.MinValue;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
            serializer.SerializeValue(ref MaxValue);
            serializer.SerializeValue(ref MinValue);
        }
    }

    /// <summary>
    /// <c>FloatCappedStat</c> represents a stat an entity involved in combat has that includes some maximum and minimum value.
    /// </summary>
    public struct FloatCappedStat : INetworkSerializable
    {
        public float BaseValue;
        public float CurrentValue;
        public float MaxValue;
        public float MinValue;

        public FloatCappedStat(FloatCappedStat toCopy, float newCurrent)
        {
            BaseValue = toCopy.BaseValue;
            CurrentValue = newCurrent;
            MaxValue = toCopy.MaxValue;
            MinValue = toCopy.MinValue;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
            serializer.SerializeValue(ref MaxValue);
            serializer.SerializeValue(ref MinValue);
        }
    }

    /// <summary>
    /// <c>VariableStat</c> represents a stat that can vary due to alterations by other game systems.
    /// </summary>
    public struct VariableStat : INetworkSerializable
    {
        public int BaseValue;
        public int CurrentValue;

        public VariableStat(VariableStat toCopy, int newCurrent)
        {
            BaseValue = toCopy.BaseValue;
            CurrentValue = newCurrent;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }

    /// <summary>
    /// <c>FloatVariableStat</c> represents a stat that can vary due to alterations by other game systems.
    /// </summary>
    public struct FloatVariableStat : INetworkSerializable
    {
        public float BaseValue;
        public float CurrentValue;

        public FloatVariableStat(FloatVariableStat toCopy, float newCurrent)
        {
            BaseValue = toCopy.BaseValue;
            CurrentValue = newCurrent;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref BaseValue);
            serializer.SerializeValue(ref CurrentValue);
        }
    }
}
