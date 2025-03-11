using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Services;
using OpenRP.Framework.Features.Commands.Attributes;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Services;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.CDN.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Commands
{
    public class CollectEggCommand : ISystem
    {
        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Collect an egg from a chicken coop. Use this command to gather eggs if you're near a chicken coop.")]
        public void CollectEgg(Player player, IEntityManager entityManager, ITempCharacterService characterService, IInventoryService inventoryService, IChickenCoopManager chickenCoopManager, IChatService chatService, IDataMemoryService dataMemoryService, IOpenCdnService openCdnService)
        {
            foreach (ChickenCoop.Components.ChickenCoop chickenCoop in chickenCoopManager.GetAllChickenCoops())
            {
                if(chickenCoop.IsPlayerNearby(player))
                {
                    chickenCoop.CollectEgg(player, entityManager, characterService, inventoryService, chatService, dataMemoryService, openCdnService);
                } else
                {
                    player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You must be near a chicken coop to do this command!");
                }
            }
        }
    }
}
