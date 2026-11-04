using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Inventories.Components;
using OpenRP.Framework.Features.Inventories.Entities;
using OpenRP.Framework.Features.Inventories.Helpers;
using OpenRP.Framework.Features.Items.Components;
using OpenRP.Framework.Features.Items.Entities;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Boilerplate.Data;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Streamer;
using System.Linq;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Components
{
    public class DroppedItem : Component
    {
        private ulong _databaseId;
        private bool _pickupable;
        private DroppedInventoryItemModel _cachedDroppedItemModel;
        public DroppedItem(DroppedInventoryItemModel droppedItemModel)
        {
            _cachedDroppedItemModel = droppedItemModel;
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

        public bool Pickup(Player player)
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

            InventoryItemModel droppedInventoryItem = _cachedDroppedItemModel.InventoryItem;
            Character character = player.GetPlayerCurrentlyPlayingAsCharacter();
            Inventory? inventory = character.GetInventory();
            Item? item = Manager.GetComponent<Item>(ItemEntities.GetItemId((int)droppedInventoryItem.ItemId));
            if (inventory == null || item == null || !inventory.AddItem(item, droppedInventoryItem.Amount, ItemAdditionalData.Parse(droppedInventoryItem.AdditionalData)))
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You don't have enough space in your inventory to pick this up!");
                return false;
            }

            _pickupable = false;
            DeleteDroppedInventoryItem(droppedInventoryItem.Id);
            player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"You have picked up {droppedInventoryItem.Amount}x {item.GetName()}.");

            DestroyEntity();
            return true;
        }

        /// <summary>
        /// Removes the dropped item from the world inventory, in the database and in memory.
        /// </summary>
        private void DeleteDroppedInventoryItem(ulong inventoryItemId)
        {
            using (DataContext context = new DataContext())
            {
                context.InventoryItems
                    .Where(invItem => invItem.Id == inventoryItemId)
                    .ExecuteDelete();
            }

            EntityId inventoryItemEntityId = InventoryEntities.GetInventoryItemId((int)inventoryItemId);
            if (Manager.Exists(inventoryItemEntityId))
            {
                Manager.Destroy(inventoryItemEntityId);
            }
        }
    }
}
