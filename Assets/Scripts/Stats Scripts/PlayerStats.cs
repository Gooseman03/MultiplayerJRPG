using UnityEngine;

namespace Ladder.EntityStatistics
{
    public class PlayerStats : EntityStats
    {
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
            base.InitializeValues(); // Initializes all regular values associated with all entities
        }

        public override void SetupOnChangedEvents()
        {
            base.SetupOnChangedEvents();
        }
    }
}
