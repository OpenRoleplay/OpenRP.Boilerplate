using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Boilerplate.Data;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers
{
    public static class InventoryItemHelper
    {
        public static InventoryModel GetItemInventory(this InventoryItemModel item, IDataMemoryService dataMemoryService)
        {
            using (var context = new DataContext())
            {
                if (item.GetItem(dataMemoryService).IsItemInventory())
                {
                    ItemAdditionalData itemAdditionalData = ItemAdditionalData.Parse(item.AdditionalData);

                    string stringInventoryId = itemAdditionalData.GetString("INVENTORY");
                    if (String.IsNullOrEmpty(stringInventoryId))
                    {
                        // MIGRATION FIX: InventoryModel inventory = InventoryHelper.CreateInventory(item.GetItem().Name, 1000);
                        InventoryModel inventory = null;
                        if (inventory != null)
                        {
                            stringInventoryId = inventory.Id.ToString();
                            itemAdditionalData.SetString("INVENTORY", stringInventoryId);

                            InventoryItemModel updateItem = context.InventoryItems.Find(item.Id);
                            updateItem.AdditionalData = item.AdditionalData = itemAdditionalData.ToString();
                            context.SaveChanges();
                        }
                    }

                    if (ulong.TryParse(stringInventoryId, out ulong inventoryId))
                    {
                        return context.Inventories
                            .Include(i => i.Items)
                            .FirstOrDefault(i => i.Id == inventoryId);
                    }
                }
                return null;
            }
        }

        public static InventoryModel GetInventoryIn(this InventoryItemModel item)
        {
            using (var context = new DataContext())
            {
                return context.Inventories.Find(item.InventoryId);
            }
        }

        public static string GetName(this InventoryItemModel inventoryItem, IDataMemoryService dataMemoryService)
        {
            return inventoryItem.GetItem(dataMemoryService).Name;
        }

        public static uint GetWeight(this InventoryItemModel inventoryItem, IDataMemoryService dataMemoryService)
        {
            if(inventoryItem.Weight != null)
            {
                return inventoryItem.Weight.Value;
            }

            return inventoryItem.GetItem(dataMemoryService).Weight;
        }

        public static ItemModel GetItem(this InventoryItemModel inventoryItem, IDataMemoryService dataMemoryService)
        {
            return dataMemoryService.GetItems().FirstOrDefault(i => i.Id == inventoryItem.ItemId);
        }


        public static uint GetTotalWeight(this InventoryItemModel inventoryItem, uint amount, IDataMemoryService dataMemoryService)
        {
            uint total_weight = inventoryItem.GetWeight(dataMemoryService) * amount;

            if (inventoryItem.GetItem(dataMemoryService).IsItemInventory())
            {
                total_weight += (uint)inventoryItem.GetItemInventory(dataMemoryService).GetInventoryItems().Sum(i => i.GetTotalWeight(dataMemoryService));
            }

            return total_weight;
        }

        public static uint GetTotalWeight(this InventoryItemModel inventoryItem, IDataMemoryService dataMemoryService)
        {
            return GetTotalWeight(inventoryItem, inventoryItem.Amount, dataMemoryService);
        }

        public static bool DoesInventoryItemFitInInventory(this InventoryItemModel inventoryItemToCheck, InventoryModel inventoryToCheck, uint amountToCheck, IDataMemoryService dataMemoryService)
        {
            // Check if amount of inventoryitem fits in the inventory 
            if (inventoryItemToCheck.GetTotalWeight(amountToCheck, dataMemoryService) < inventoryToCheck.GetAvailableWeight(dataMemoryService))
            {
                return true;
            }
            return false;
        }

        public static bool Remove(this InventoryItemModel inventoryItem, uint amount)
        {
            try
            {
                using (var context = new DataContext())
                {
                    context.InventoryItems.Attach(inventoryItem);

                    if (amount == inventoryItem.Amount)
                    {
                        // Remove the entire InventoryItemModel from the database.
                        context.InventoryItems.Remove(inventoryItem);
                    }
                    else if (amount < inventoryItem.Amount)
                    {
                        inventoryItem.Amount -= amount;
                    }

                    int changes = context.SaveChanges();
                    if (changes > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        public static bool Transfer(this InventoryItemModel inventoryItem, ulong toInventoryId, uint amount, IDataMemoryService dataMemoryService)
        {
            using (var context = new DataContext())
            {
                // Load the destination inventory
                InventoryModel newInventory = context.Inventories.SingleOrDefault(i => i.Id == toInventoryId);
                if (newInventory == null)
                {
                    throw new ArgumentException($"Inventory with ID {toInventoryId} does not exist.");
                }

                // Check if the item fits in the new inventory
                if (!inventoryItem.DoesInventoryItemFitInInventory(newInventory, amount, dataMemoryService))
                {
                    return false;
                }

                if (inventoryItem.Amount == amount)
                {
                    // Move the entire inventory item to the new inventory
                    inventoryItem.InventoryId = toInventoryId;
                }
                else if (inventoryItem.Amount > amount)
                {
                    // Reduce the amount in the current inventory item
                    inventoryItem.Amount -= amount;

                    // Create a new inventory item for the transferred amount
                    InventoryItemModel newInventoryItem = new InventoryItemModel
                    {
                        ItemId = inventoryItem.ItemId,
                        Amount = amount,
                        InventoryId = toInventoryId,
                        AdditionalData = inventoryItem.AdditionalData,
                        CanDestroy = inventoryItem.CanDestroy,
                        CanDrop = inventoryItem.CanDrop,
                        KeepOnDeath = inventoryItem.KeepOnDeath,
                        UsesRemaining = inventoryItem.UsesRemaining,
                        Weight = inventoryItem.Weight,
                    };

                    context.InventoryItems.Add(newInventoryItem);
                }
                else
                {
                    throw new InvalidOperationException("Not enough items to transfer.");
                }

                // Save changes to the database
                int changes = context.SaveChanges();
                return changes > 0;
            }
        }

        public static bool Transfer(this InventoryItemModel inventoryItem, ulong toInventoryId, IDataMemoryService dataMemoryService)
        {
            return Transfer(inventoryItem, toInventoryId, inventoryItem.Amount, dataMemoryService);
        }
    }
}
