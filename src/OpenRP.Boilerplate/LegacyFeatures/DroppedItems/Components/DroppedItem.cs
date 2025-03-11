using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.Streamer.Entities;
using OpenRP.Boilerplate.Data;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Components
{
    public class DroppedItem : Component
    {
        private ulong _databaseId;
        private bool _pickupable;
        private DroppedInventoryItemModel _cachedDroppedItemModel;
        private IInventoryService _inventoryService;
        private ITempCharacterService _characterService;
        public DroppedItem(DroppedInventoryItemModel droppedItemModel, IInventoryService inventoryService, ITempCharacterService characterService)
        {
            _cachedDroppedItemModel = droppedItemModel;
            _inventoryService = inventoryService;
            _characterService = characterService;
            _databaseId = _cachedDroppedItemModel.Id;
            _pickupable = true;
        }

        public ulong GetDatabaseId()
        {
            return _databaseId;
        }

        public DynamicObject GetDynamicObject()
        {
            return this.GetComponentInChildren<DynamicObject>();
        }

        public DynamicTextLabel GetDynamicTextLabel()
        {
            return this.GetComponentInChildren<DynamicTextLabel>();
        }

        public bool IsPlayerNearby(Player player)
        {
            DynamicObject dynamicObject = GetDynamicObject();
            Vector3 checkPosition = new Vector3(dynamicObject.Position.XY, dynamicObject.Position.Z + 0.25f);
            if (player.IsInRangeOfPoint(1.5f, dynamicObject.Position))
            {
                return true;
            }
            return false;
        }

        public bool Pickup(Player player, IDataMemoryService dataMemoryService)
        {
            if (!IsPlayerNearby(player))
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You are not nearby any items to pickup!");
                return false;
            }

            if (!_pickupable)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "This item cannot be picked up! Maybe someone else is already picking it up?");
                return false;
            }

            _pickupable = false;

            Character character = player.GetPlayerCurrentlyPlayingAsCharacter();
            InventoryModel inventoryModel = _characterService.GetCharacterInventory(character);

            uint amount = _cachedDroppedItemModel.InventoryItem.Amount;

            // Create a new instance for the player's inventory, copying relevant properties
            InventoryItemModel newInventoryItem = _inventoryService.PrepareItem(inventoryModel.Id, _cachedDroppedItemModel.InventoryItem, _cachedDroppedItemModel.InventoryItem.Amount);

            InventoryItemModel itemAdded = _inventoryService.AddItem(newInventoryItem);

            // Use a new DataContext to delete the dropped item record
            using (DataContext context = new DataContext())
            {
                context.InventoryItems
                    .Where(invItem => invItem.Id == _cachedDroppedItemModel.InventoryItemId)
                    .ExecuteDelete();
            }

            player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"You have picked up {amount}x {itemAdded.GetItem(dataMemoryService).Name}.");

            DestroyEntity();
            return true;
        }

    }
}
