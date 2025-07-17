using System;
using Unity.Netcode;
using UnityEngine;
using Ladder.Netcode.StatTypes;
using Ladder.Netcode.Primitives;

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
    public abstract class EntityStats : NetworkBehaviour
    {

        // Level and general stats
        public NetworkVariable<int> Level = new();
        [SerializeField] private const int baseLevel = 1;

        // Health Values
        public CappedStat<int> Health = new();
        //public NetworkVariable<CappedStat<NetworkedInt>> Health = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseHealth = 100;

        // Attack Values
        public VariableStat<int> PhysicalAttack = new();
        //public NetworkVariable<VariableStat<NetworkedInt>> PhysicalAttack = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int basePhysicalAttack = 10;

        public VariableStat<int> MagicalAttack = new();
        //public NetworkVariable<VariableStat<NetworkedInt>> MagicalAttack = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseMagicalAttack = 10;

        // Defense Values
        public VariableStat<int> PhysicalDefense = new();
        //public NetworkVariable<VariableStat<NetworkedInt>> PhysicalDefense = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int basePhysicalDefense = 10;

        public VariableStat<int> MagicalDefense = new();
        //public NetworkVariable<VariableStat<NetworkedInt>> MagicalDefense = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseMagicalDefense = 10;

        // Speed Values
        public VariableStat<int> Speed = new();
        //public NetworkVariable<VariableStat<NetworkedInt>> Speed = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseSpeed = 5;

        // Crit/Luck
        public VariableStat<float> CritRate = new();
        //public NetworkVariable<VariableStat<NetworkedFloat>> CritRate = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const float baseCritRate = 0.05f;

        public VariableStat<float> CritDamage = new();
        //public NetworkVariable<VariableStat<NetworkedFloat>> CritDamage = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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

            Health.CurrentValue = baseHealth;
            Health.MaxValue = baseHealth;
            Health.BaseValue = baseHealth;
            Health.MinValue = 0;

            PhysicalAttack.CurrentValue = basePhysicalAttack;
            PhysicalAttack.BaseValue = basePhysicalAttack;

            MagicalAttack.CurrentValue = baseMagicalAttack;
            MagicalAttack.BaseValue = baseMagicalAttack;

            PhysicalDefense.CurrentValue = basePhysicalDefense;
            PhysicalDefense.BaseValue = basePhysicalDefense;

            MagicalDefense.CurrentValue = baseMagicalDefense;
            MagicalDefense.BaseValue = baseMagicalDefense;

            Speed.CurrentValue = baseSpeed;
            Speed.BaseValue = baseSpeed;

            CritRate.CurrentValue = baseCritRate;
            CritRate.BaseValue = baseCritRate;

            CritDamage.CurrentValue = baseCritDamage;
            CritDamage.BaseValue = baseCritDamage;
            /*Health.Value = new CappedStat<NetworkedInt>() 
            {
                BaseValue = baseHealth, CurrentValue = baseHealth, MaxValue = baseHealth, MinValue = 0
            };
            PhysicalAttack.Value = new VariableStat<NetworkedInt>()
            {
                BaseValue = basePhysicalAttack, CurrentValue = basePhysicalAttack
            };
            MagicalAttack.Value = new VariableStat<NetworkedInt>()
            {
                BaseValue = baseMagicalAttack, CurrentValue = baseMagicalAttack
            };
            PhysicalDefense.Value = new VariableStat<NetworkedInt>()
            {
                BaseValue = basePhysicalDefense, CurrentValue = basePhysicalDefense
            };
            MagicalDefense.Value = new VariableStat<NetworkedInt>()
            {
                BaseValue = baseMagicalDefense, CurrentValue = baseMagicalDefense
            };
            Speed.Value = new VariableStat<NetworkedInt>()
            {
                BaseValue = baseSpeed, CurrentValue = baseSpeed
            };
            CritRate.Value = new VariableStat<NetworkedFloat>()
            {
                BaseValue = baseCritRate, CurrentValue = baseCritRate
            };
            CritDamage.Value = new VariableStat<NetworkedFloat>()
            {
                BaseValue = baseCritDamage, CurrentValue = baseCritDamage
            };*/
        }

        // Attaches all events to proper channels.
        public virtual void SetupOnChangedEvents()
        {
            return;
        }


        // Alters the maximum health by specified amount. Scales owner's current health if specified by boolean.
        public virtual void AlterMaxHealth(int alteration, bool shouldAlterCurrent = true)
        {
            Health.MaxValue += alteration;
            if (Health.MaxValue <= Health.CurrentValue)
            {
                Health.CurrentValue = Health.MaxValue;
            }

            /*int temp = Health.Value.MaxValue + alteration;
            if (temp <= Health.Value.CurrentValue)
            {
                Health.Value = new()
                {
                    CurrentValue = temp,
                    BaseValue = Health.Value.BaseValue,
                    MaxValue = temp,
                    MinValue = Health.Value.MinValue
                };
            }*/
        }

        // Resets the maximum health to the stored base value. Sets current health to maximum if specified by boolean or is greater than base health.
        public virtual void ResetMaxHealth(bool resetCurrentHealth)
        {
            if (resetCurrentHealth || Health.CurrentValue > Health.BaseValue)
            {
                Health.CurrentValue = Health.BaseValue;
            }
            Health.MaxValue = Health.BaseValue;

            /*if (resetCurrentHealth || Health.Value.CurrentValue > Health.Value.BaseValue)
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
            }*/
        }


        // Alters the attack stat of the specified damage type.
        public virtual void AlterAttackOfType(DamageType damageType, int alteration)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalAttack.CurrentValue += alteration;

                    /*PhysicalAttack.Value = new()
                    {
                        CurrentValue = PhysicalAttack.Value.CurrentValue + alteration,
                        BaseValue = PhysicalAttack.Value.BaseValue
                    };*/
                    break;

                case DamageType.magical:
                    MagicalAttack.CurrentValue += alteration;
                    /*MagicalAttack.Value = new()
                    {
                        CurrentValue = MagicalAttack.Value.CurrentValue + alteration,
                        BaseValue = MagicalAttack.Value.BaseValue
                    };*/
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify the amount you are altering the attack of.");
            }
        }

        // Resets the attack modifier of the specified damage type.
        public virtual void ResetAttackOfType(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalAttack.CurrentValue = PhysicalAttack.BaseValue;
                    /*PhysicalAttack.Value = new()
                    {
                        CurrentValue = PhysicalAttack.Value.BaseValue,
                        BaseValue = PhysicalAttack.Value.BaseValue
                    };*/
                    break;
                
                case DamageType.magical:
                    MagicalAttack.CurrentValue = MagicalAttack.BaseValue;
                    /*MagicalAttack.Value = new()
                    {
                        CurrentValue = MagicalAttack.Value.BaseValue,
                        BaseValue = PhysicalAttack.Value.BaseValue
                    };*/
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify attack damage type to reset.");
            }
        }


        // Alters the crit rate by the specified alteration.
        public virtual void AlterCritRate(float alteration)
        {
            CritRate.CurrentValue += alteration;
            /*CritRate.Value = new()
            {
                CurrentValue = Math.Max(CritRate.Value.CurrentValue + alteration, 0f),
                BaseValue = CritRate.Value.BaseValue
            };*/
        }

        // Resets the crit rate back to the specified base value.
        public virtual void ResetCritRate()
        {
            CritRate.CurrentValue = CritRate.BaseValue;
            /*CritRate.Value = new()
            {
                CurrentValue = CritRate.Value.BaseValue,
                BaseValue = CritRate.Value.BaseValue
            };*/
        }


        // Alters the crit damage by the specified amount.
        public virtual void AlterCritDamage(float alteration)
        {
            CritDamage.CurrentValue += alteration;
            /*CritDamage.Value = new()
            {
                CurrentValue = Math.Max(CritDamage.Value.CurrentValue + alteration, 1f),
                BaseValue = CritDamage.Value.BaseValue
            };*/
        }

        // Resets the crit damage to it's specified default value.
        public virtual void ResetCritDamage()
        {
            CritDamage.CurrentValue = CritDamage.BaseValue;
            /*CritDamage.Value = new()
            {
                CurrentValue = CritDamage.Value.BaseValue,
                BaseValue = CritDamage.Value.BaseValue
            };*/
        }


        // Defense Methods
        // Alters the defense of the specified damage type by the specified amount.
        public virtual void AlterDefenseOfType(DamageType damageType, int alteration)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalDefense.CurrentValue += alteration;
                    /*PhysicalDefense.Value = new()
                    {
                        CurrentValue = PhysicalDefense.Value.CurrentValue + alteration,
                        BaseValue = PhysicalDefense.Value.BaseValue
                    };*/
                    break;

                case DamageType.magical:
                    MagicalDefense.CurrentValue += alteration;
                    /*MagicalDefense.Value = new()
                    {
                        CurrentValue = MagicalDefense.Value.CurrentValue + alteration,
                        BaseValue = MagicalDefense.Value.BaseValue
                    };*/
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify the amount you are altering the defense of.");
            }
        }

        // Resets the defense of the specified damage type to its specified base value.
        public virtual void ResetDefenseOfType(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.physical:
                    PhysicalDefense.CurrentValue = PhysicalDefense.BaseValue;
                    /*PhysicalDefense.Value = new()
                    {
                        CurrentValue = PhysicalDefense.Value.BaseValue,
                        BaseValue = PhysicalDefense.Value.BaseValue
                    };*/
                    break;

                case DamageType.magical:
                    MagicalDefense.CurrentValue = MagicalDefense.BaseValue;
                    /*MagicalDefense.Value = new()
                    {
                        CurrentValue = MagicalDefense.Value.BaseValue,
                        BaseValue = MagicalDefense.Value.BaseValue
                    };*/
                    break;

                case DamageType.trueDamage:
                    break;

                default:
                    throw new ArgumentException("Please specify defense type to reset.");
            }
        }


        // Speed Methods
        // Alters the speed by the specified amount.
        public virtual void AlterSpeed(int alteration)
        {
            Speed.CurrentValue += alteration;
            /*Speed.Value = new()
            {
                CurrentValue = Speed.Value.CurrentValue + alteration,
                BaseValue = Speed.Value.BaseValue
            };*/
        }

        // Resets the speed to the stored base value.
        public virtual void ResetSpeed()
        {
            Speed.CurrentValue = Speed.BaseValue;
            /*Speed.Value = new()
            {
                CurrentValue = Speed.Value.BaseValue,
                BaseValue = Speed.Value.BaseValue
            };*/
        }
    }
}
