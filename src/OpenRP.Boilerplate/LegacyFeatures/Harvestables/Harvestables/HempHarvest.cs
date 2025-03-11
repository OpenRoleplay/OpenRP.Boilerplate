using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Components;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Entities;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Harvestables.Harvestables
{
    public class HempHarvest : IHarvestable
    {
        private IEntityManager _entityManager;
        public HempHarvest(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public string ResourceName => "hemp";

        public void Harvest(Player player)
        {
            foreach (IndianHempPlant indianHempPlant in _entityManager.GetComponents<IndianHempPlant>())
            {
                if (indianHempPlant.IsPlayerNearby(player))
                {
                    PlayerHarvesting.StartHarvesting(player, this, indianHempPlant, TimeSpan.FromSeconds(10));
                    return;
                }
            }
            player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You are not near any harvestable Indian Hemp plants!");
        }

        public void Harvested(Player player)
        {
        }
    }

}
