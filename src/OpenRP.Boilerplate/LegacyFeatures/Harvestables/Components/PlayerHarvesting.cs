using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Entities;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Harvestables.Components
{
    public class PlayerHarvesting : Component
    {
        private readonly Player _player;
        private readonly IHarvestable _harvesting;
        private readonly IHarvestablePlant _harvestingPlant;
        private readonly DateTime _finished;
        public PlayerHarvesting(Player player, IHarvestable harvesting, IHarvestablePlant harvestingPlant, TimeSpan duration)
        {
            _player = player;
            _harvesting = harvesting;
            _harvestingPlant = harvestingPlant;
            _finished = DateTime.UtcNow.Add(duration);
        }

        public bool IsDone()
        {
            return DateTime.UtcNow > _finished;
        }

        public void Finish()
        {
            if (IsDone())
            {
                _harvesting.Harvested(_player);
                _harvestingPlant.Harvest();
                _player.ClearAnimations();
                _player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"You have harvested 1x {_harvesting.ResourceName}.");
                Destroy();
            }
        }

        public static void StartHarvesting(Player player, IHarvestable harvestable, IHarvestablePlant harvestingPlant, TimeSpan duration)
        {
            player.AddComponent<PlayerHarvesting>(player, harvestable, harvestingPlant, duration);

            player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"You are now harvesting {harvestable.ResourceName}.");

            player.ApplyAnimation("BOMBER", "BOM_Plant_Loop", 4.1f, true, false, false, true, 0);
        }
    }
}
