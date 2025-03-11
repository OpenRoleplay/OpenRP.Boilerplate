using OpenRP.Framework.Features.Commands.Attributes;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Entities;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;

namespace OpenRP.Boilerplate.LegacyFeatures.Harvestables.Commands
{
    public class HarvestCommand : ISystem
    {
        private readonly IEnumerable<IHarvestable> _harvestableResources;

        public HarvestCommand(IEnumerable<IHarvestable> harvestableResources)
        {
            _harvestableResources = harvestableResources;
        }

        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Harvest a resource.",
            CommandGroups = new string[] { "Gathering" })]
        public void Harvest(Player player, string resource)
        {
            // Look for a harvestable provider that matches the resource name.
            var harvestable = _harvestableResources.FirstOrDefault(r =>
                r.ResourceName.Equals(resource, StringComparison.OrdinalIgnoreCase));

            if (harvestable != null)
            {
                // Delegate to the resource's own harvest logic.
                harvestable.Harvest(player);
            }
            else
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, $"No harvestable resource found for '{resource}'.");
            }
        }

        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Harvest a resource.",
            CommandGroups = new string[] { "Gathering" })]
        public void Harvest(Player player)
        {
            IEnumerable<string> harvestable = _harvestableResources.Select(i => i.ResourceName);
            string options = string.Join($" {ChatColor.Yellow}OR{ChatColor.White} ", harvestable);
            player.SendPlayerInfoMessage(PlayerInfoMessageType.SYNTAX, $"/harvest [{options}]");
        }
    }
}
