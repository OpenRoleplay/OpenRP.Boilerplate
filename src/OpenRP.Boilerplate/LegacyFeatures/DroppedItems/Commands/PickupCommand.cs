using OpenRP.Framework.Features.Commands.Attributes;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Components;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Database.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Commands
{
    public class PickupCommand : ISystem
    {
        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Pickup a dropped item from the ground.",
            CommandGroups = new string[] { "Inventory" })]
        public async void Pickup(Player player, IDroppedItemService droppedItemService, IDataMemoryService dataMemoryService)
        {
            foreach(DroppedItem droppedItem in droppedItemService.GetAllDroppedItems())
            {
                if(droppedItem.IsPlayerNearby(player))
                {
                    droppedItem.Pickup(player, dataMemoryService);
                    return;
                }
            }

            player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You are not near any dropped items!");
        }
    }
}
