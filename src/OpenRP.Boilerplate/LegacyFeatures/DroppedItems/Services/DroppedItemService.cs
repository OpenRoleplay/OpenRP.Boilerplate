using OpenRP.Boilerplate.Data;
using SampSharp.Entities;
using SampSharp.Streamer.Entities;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Components;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Entities;
using Microsoft.EntityFrameworkCore;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using SampSharp.Entities.SAMP;
using SampSharp.Tryg3D.Entities.Services;
using SampSharp.ColAndreas.Entities.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.VirtualWorlds.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services
{
    public class DroppedItemService : IDroppedItemService
    {
        private IEntityManager _entityManager;
        private IStreamerService _streamerService;
        private IVirtualWorldManager _virtualWorldManager;
        private ITryg3DService _tryg3DService;
        private IColAndreasService _colAndreasService;
        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        private IServerEventAggregator _serverEventAggregator;
        private ITempCharacterService _tempCharacterService;
        private IDataMemoryService _dataMemoryService;

        public DroppedItemService(IEntityManager entityManager, IStreamerService streamerService, IVirtualWorldManager virtualWorldManager, ITryg3DService tryg3DService, IColAndreasService colAndreasService, IInventoryService inventoryService, ICharacterService characterService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IDataMemoryService dataMemoryService)
        {
            _entityManager = entityManager;
            _streamerService = streamerService;
            _virtualWorldManager = virtualWorldManager;
            _tryg3DService = tryg3DService;
            _colAndreasService = colAndreasService;
            _inventoryService = inventoryService;
            _characterService = characterService;
            _serverEventAggregator = serverEventAggregator;
            _tempCharacterService = tempCharacterService;
            _dataMemoryService = dataMemoryService;
        }

        public void LoadDroppedItems()
        {
            List<ulong> currentlyLoadedDroppedItemIds = GetCurrentlyLoadedDroppedItemDatabaseIds();

            // Fetch all vehicles except those with the specified IDs
            using (DataContext context = new DataContext())
            {
                List<DroppedInventoryItemModel> droppedItems = context.DroppedInventoryItems
                    .Include(droppedItem => droppedItem.InventoryItem)
                    .Where(droppedItem => !currentlyLoadedDroppedItemIds.Contains(droppedItem.Id))
                    .ToList();

                foreach (DroppedInventoryItemModel droppedItem in droppedItems)
                {
                    DroppedItem loadedProperty = LoadDroppedItem(droppedItem);
                }
            }
        }

        public DroppedItem LoadDroppedItem(DroppedInventoryItemModel droppedItemModel)
        {
            // Add Component
            EntityId droppedItemEntityId = DroppedItemEntities.GetDroppedItemId((int)droppedItemModel.Id);
            _entityManager.Create(droppedItemEntityId);

            DroppedItem droppedItem = _entityManager.AddComponent<DroppedItem>(droppedItemEntityId, droppedItemModel, _inventoryService, _characterService);

            // Place Dynamic Object
            int DroppedItemModel = 1575;
            int? CustomDroppedItemModel = droppedItemModel.InventoryItem.GetItem(_dataMemoryService).DropModelId;
            if (CustomDroppedItemModel != null)
            {
                DroppedItemModel = CustomDroppedItemModel.Value;
            }

            _streamerService.CreateDynamicObject(
                DroppedItemModel
                , new SampSharp.Entities.SAMP.Vector3(
                    droppedItemModel.PosX
                    , droppedItemModel.PosY
                    , droppedItemModel.PosZ
                )
                , new SampSharp.Entities.SAMP.Vector3(
                    droppedItemModel.RotX.Value
                    , droppedItemModel.RotY.Value
                    , droppedItemModel.RotZ.Value
                )
                , parent: droppedItemEntityId
            );

            _streamerService.CreateDynamicTextLabel(
                $"{droppedItemModel.InventoryItem.Amount}x {droppedItemModel.InventoryItem.GetItem(_dataMemoryService).Name}\n/pickup to pickup"
                , Color.WhiteSmoke
                , new SampSharp.Entities.SAMP.Vector3(
                    droppedItemModel.PosX
                    , droppedItemModel.PosY
                    , droppedItemModel.PosZ + 0.25f
                )
                , 1.5f
                , testLos: true
                , parent: droppedItemEntityId
            );

            return droppedItem;
        }

        private List<ulong> GetCurrentlyLoadedDroppedItemDatabaseIds()
        {
            List<ulong> loadedDroppedItemDatabaseIds = new List<ulong>();
            foreach (DroppedItem loadedDroppedItem in _entityManager.GetComponents<DroppedItem>())
            {
                loadedDroppedItemDatabaseIds.Add(loadedDroppedItem.GetDatabaseId());
            }
            return loadedDroppedItemDatabaseIds;
        }

        public IEnumerable<DroppedItem> GetAllDroppedItems()
        {
            return _entityManager.GetComponents<DroppedItem>();
        }

        public bool DropItem(Player player, InventoryItemModel inventoryItem, int? dropQuantity = null, int spawnInRange = 0)
        {
            try
            {
                using (DataContext context = new DataContext())
                {
                    // Reattach the item from the DB.
                    var item = context.InventoryItems.Find(inventoryItem.Id);
                    if (item == null)
                        return false;

                    // Determine the spawn position.
                    Vector3 spawnPos = player.Position;
                    if (spawnInRange > 0)
                    {
                        // Get a random point within a circle around the player's XY position.
                        // (Assumes _tryg3DService.GetPointInCircle returns a Vector2.)
                        Vector2 randomPoint = _tryg3DService.GetPointInCircle(player.Position.XY, spawnInRange);
                        spawnPos = new Vector3(randomPoint.X, randomPoint.Y, player.Position.Z);
                    }

                    // Perform a raycast to attempt to get proper rotation.
                    // The ray cast goes from spawnPos downwards. Adjust as needed.
                    Vector3 hitPosition, hitRotation;
                    int detected = _colAndreasService.RayCastLineAngle(
                        spawnPos,
                        new Vector3(player.Position.XY, -100),
                        out hitPosition,
                        out hitRotation);
                    if (detected == 0)
                    {
                        // If nothing was hit, adjust Z coordinate.
                        hitPosition = _colAndreasService.FindZ_For2DCoord(spawnPos);
                        // Default rotation if none available.
                        hitRotation = new Vector3(0, 0, 0);
                    }

                    // Remove personalized data.
                    // Assume ItemAdditionalData.Parse() and RemovePersonalizedData() are implemented.
                    ItemAdditionalData parsedData = ItemAdditionalData.Parse(item.AdditionalData);
                    ItemAdditionalData cleanedData = ItemAdditionalData.RemovePersonalizedData(parsedData);
                    string cleanedAdditionalData = cleanedData.ToString(); // Adjust conversion if necessary.

                    // Determine full drop vs. partial drop.
                    if (!dropQuantity.HasValue || dropQuantity.Value >= item.Amount)
                    {
                        // Full drop: set InventoryId to 1 (world inventory) and update the cleaned AdditionalData.
                        item.InventoryId = 1;
                        item.AdditionalData = cleanedAdditionalData;
                        context.InventoryItems.Update(item);

                        // Create a dropped inventory item record with the position and rotation.
                        var dropped = new DroppedInventoryItemModel
                        {
                            InventoryItemId = item.Id,
                            PosX = hitPosition.X,
                            PosY = hitPosition.Y,
                            PosZ = hitPosition.Z,
                            RotX = hitRotation.X,
                            RotY = hitRotation.Y,
                            RotZ = hitRotation.Z
                        };
                        context.DroppedInventoryItems.Add(dropped);
                    }
                    else
                    {
                        // Partial drop: reduce the quantity on the existing item and clone a new one.
                        uint dropQty = (uint)dropQuantity.Value;
                        if (dropQty >= item.Amount)
                        {
                            // Treat as full drop if quantity is equal to or exceeds available amount.
                            item.InventoryId = 1;
                            item.AdditionalData = cleanedAdditionalData;
                            context.InventoryItems.Update(item);

                            // Create a dropped inventory item record with the position and rotation.
                            var dropped = new DroppedInventoryItemModel
                            {
                                InventoryItemId = item.Id,
                                PosX = hitPosition.X,
                                PosY = hitPosition.Y,
                                PosZ = hitPosition.Z,
                                RotX = hitRotation.X,
                                RotY = hitRotation.Y,
                                RotZ = hitRotation.Z
                            };
                            context.DroppedInventoryItems.Add(dropped);
                        }
                        else
                        {
                            // Subtract dropQty from the existing item.
                            _inventoryService.RemoveItem(item, dropQty);

                            // Create a new InventoryItemModel for the dropped portion.
                            var newItem = new InventoryItemModel
                            {
                                ItemId = item.ItemId,
                                Amount = dropQty,
                                UsesRemaining = item.UsesRemaining,
                                KeepOnDeath = item.KeepOnDeath,
                                CanDrop = item.CanDrop,
                                CanDestroy = item.CanDestroy,
                                InventoryId = 1, // World inventory.
                                AdditionalData = cleanedAdditionalData
                            };
                            context.InventoryItems.Add(newItem);

                            context.SaveChanges();

                            // Create a dropped inventory item record with the position and rotation.
                            var dropped = new DroppedInventoryItemModel
                            {
                                InventoryItemId = newItem.Id,
                                PosX = hitPosition.X,
                                PosY = hitPosition.Y,
                                PosZ = hitPosition.Z,
                                RotX = hitRotation.X,
                                RotY = hitRotation.Y,
                                RotZ = hitRotation.Z
                            };
                            context.DroppedInventoryItems.Add(dropped);
                        }
                    }

                    context.SaveChanges();
                }

                if(inventoryItem.GetItem(_dataMemoryService).IsItemCurrency())
                {
                    Character character = player.GetPlayerCurrentlyPlayingAsCharacter();
                    CharacterPreferencesModel preferences = _characterService.GetCharacterPreferences(character);
                    var eventArgsToSend = new OnPlayerCashUpdateEventArgs
                    {
                        Player = player,
                        CurrencyId = preferences.DefaultCurrencyId,
                    };
                    _serverEventAggregator.PublishAsync(eventArgsToSend);
                }

                // Optionally reload dropped items into memory.
                LoadDroppedItems();

                return true;
            }
            catch (Exception ex)
            {
                // Log exception as needed.
                return false;
            }
        }
    }
}
