using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Services
{
    public class InventoryService : IInventoryService
    {
        private DataContext _dataContext;
        private IServerEventAggregator _serverEventAggregator;
        private IDataMemoryService _dataMemoryService;
        public InventoryService(DataContext dataContext, IServerEventAggregator serverEventAggregator, IDataMemoryService dataMemoryService) 
        {
            _dataContext = dataContext;
            _serverEventAggregator = serverEventAggregator;
            _dataMemoryService = dataMemoryService;
        }

        public InventoryItemModel AddItem(InventoryItemModel item)
        {
            InventoryItemModel inventoryItemFound = HasItem(item);
            if (inventoryItemFound == null || !item.GetItem(_dataMemoryService).Stackable)
            {
                inventoryItemFound = _dataContext.InventoryItems.Add(item).Entity;
            } else {
                inventoryItemFound.Amount += item.Amount;

                _dataContext.InventoryItems.Update(inventoryItemFound);
            }
            _dataContext.SaveChanges();

            return inventoryItemFound;
        }

        public InventoryItemModel AddItem(ulong inventoryId, ItemModel item, uint amount)
        {
            InventoryItemModel itemToAdd = PrepareItem(inventoryId, item, amount);

            return AddItem(itemToAdd);
        }

        public InventoryItemModel PrepareItem(ulong inventoryId, ItemModel item, uint amount)
        {
            return new InventoryItemModel()
            {
                AdditionalData = item.UseValue,
                Amount = amount,
                CanDrop = item.CanDrop,
                CanDestroy = item.CanDestroy,
                InventoryId = inventoryId,
                ItemId = item.Id,
                KeepOnDeath = item.KeepOnDeath,
                UsesRemaining = item.MaxUses,
                Weight = item.Weight,
            };
        }

        public InventoryItemModel PrepareItem(ulong inventoryId, InventoryItemModel item, uint amount)
        {
            return new InventoryItemModel()
            {
                AdditionalData = item.AdditionalData,
                Amount = amount,
                CanDrop = item.CanDrop,
                CanDestroy = item.CanDestroy,
                InventoryId = inventoryId,
                ItemId = item.Id,
                KeepOnDeath = item.KeepOnDeath,
                UsesRemaining = item.UsesRemaining,
                Weight = item.Weight,
            };
        }

        public void RemoveItem(InventoryItemModel item)
        {
            RemoveItem(item, item.Amount);
        }

        public void RemoveItem(InventoryItemModel item, uint amountToRemove)
        {
            // Find the existing inventory item using HasItem method.
            InventoryItemModel inventoryItemFound = HasItem(item);

            // If item not found, throw an exception or handle accordingly.
            if (inventoryItemFound == null)
            {
                throw new InvalidOperationException("ItemModel not found in inventory.");
            }

            // Check if there is enough quantity to remove.
            if (inventoryItemFound.Amount < amountToRemove)
            {
                throw new InvalidOperationException("Insufficient quantity to remove.");
            }

            // Subtract the specified amount.
            inventoryItemFound.Amount -= amountToRemove;

            // If the amount reaches zero, remove the item from the context.
            if (inventoryItemFound.Amount == 0)
            {
                _dataContext.InventoryItems.Remove(inventoryItemFound);
            }
            else
            {
                // Otherwise, update the existing item with the new amount.
                _dataContext.InventoryItems.Update(inventoryItemFound);
            }

            // Save changes to the database.
            _dataContext.SaveChanges();
        }

        public InventoryItemModel HasItem(InventoryItemModel item)
        {
            List<InventoryItemModel> inventoryItems = _dataContext.InventoryItems.Where(i => 
                i.InventoryId == item.InventoryId
                && i.ItemId == item.ItemId
                && i.KeepOnDeath == item.KeepOnDeath
            ).ToList();

            foreach (InventoryItemModel inventoryItem in inventoryItems)
            {
                if(ItemAdditionalData.Equals(inventoryItem.AdditionalData, item.AdditionalData))
                {
                    return inventoryItem;
                }
            }
            return null;
        }

        public bool UseItem(Player player, InventoryItemModel item)
        {
            try
            {
                using (DataContext context = new DataContext())
                {
                    if (item.UsesRemaining != null)
                    {
                        InventoryItemModel inventoryItem = context.InventoryItems.Find(item.Id);

                        inventoryItem.UsesRemaining -= 1;

                        if (inventoryItem.UsesRemaining == 0)
                        {
                            context.InventoryItems.Where(i => i.Id == item.Id).ExecuteDelete();
                        }
                        else
                        {
                            context.SaveChanges();
                        }
                    }

                    var eventArgsToSend = new OnPlayerInventoryItemUsedEventArgs
                    {
                        Player = player,
                        InventoryItem = item
                    };
                    _serverEventAggregator.PublishAsync(eventArgsToSend);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
    }
}
