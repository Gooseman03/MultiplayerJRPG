using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Ladder.EntityStatistics
{
    public class PlayerStats : EntityStats
    {

        // Experience
        public NetworkVariable<int> Experience = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> ExpMultiplier = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Resources
        public NetworkVariable<CappedStat> MagicPoints = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private const int baseMagicPoints = 30;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Collect values from save file. TODO
                InitializeValues();
            }
            else
            {
                // Don't initialize. Already done in server.
            }
        }

        public override void InitializeValues()
        {
            Experience.Value = 0;
            ExpMultiplier.Value = 1f;
            MagicPoints.Value = new()
            {
                CurrentValue = baseMagicPoints, BaseValue = baseMagicPoints, MaxValue = baseMagicPoints, MinValue = 0
            };
            base.InitializeValues(); // Initializes all regular values associated with all entities
        }

        public override void SetupOnChangedEvents()
        {
            base.SetupOnChangedEvents();
        }


        // Experience Methods
        public int IncreaseExperience(int exp)
        {
            int tempExp = Experience.Value + Mathf.RoundToInt(exp * ExpMultiplier.Value);
            int levelUps = 0;
            int neededExp = GetExpToLevel();
            while (tempExp >= neededExp)
            {
                tempExp -= neededExp;
                levelUps++;
                neededExp = GetExpToLevel(levelUps);
            }
            if (levelUps > 0) IncreaseLevel(levelUps);
            Experience.Value = tempExp;
            return levelUps;
        }

        public int GetExpToLevel(int levelMod = 0)
        {
            return 100 + Mathf.RoundToInt(Mathf.Pow(100f, Level.Value + levelMod));
        }

        public void AlterExpMultiplier(float modifier)
        {
            ExpMultiplier.Value = Math.Max(ExpMultiplier.Value + modifier, 0f);
        }

        public bool SpendMagicPoints(int mp)
        {
            int temp = MagicPoints.Value.CurrentValue - mp;
            if (temp < MagicPoints.Value.MinValue)
            {
                return false;
            }
            MagicPoints.Value = new(MagicPoints.Value, temp);
            return true;
        }
    }
}
