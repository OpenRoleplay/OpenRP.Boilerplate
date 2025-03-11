using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Boilerplate.Data;
using System.Text;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers
{
    public static class InventoryHelper
    {
        public static InventoryModel GetParentInventory(this InventoryModel inventory, IDataMemoryService dataMemoryService)
        {
            using (var context = new DataContext())
            {
                List<InventoryModel> inventories = context.Inventories
                    .Include(i => i.Items)
                    .ToList();

                return inventories.SingleOrDefault(i => i.GetInventoryItems().Any(j => j.GetItem(dataMemoryService).IsItemInventory() && ItemAdditionalData.Parse(j.AdditionalData).GetString("INVENTORY") == inventory.Id.ToString()));
            }
        }

        public static uint GetAvailableWeight(this InventoryModel inventory, IDataMemoryService dataMemoryService)
        {
            // If the inventory has no max weight, return 0
            if (inventory.MaxWeight == 0)
            {
                return 0;
            }

            if(inventory.Items == null)
            {
                return inventory.MaxWeight.Value;
            }

            // Calculate the total weight of the items in the inventory
            uint totalWeight = (uint)inventory.Items.Sum(item => item.GetTotalWeight(dataMemoryService));

            // Return the available weight by subtracting the total weight from the MaxWeight
            uint availableWeight = inventory.MaxWeight.Value - totalWeight;

            // Ensure the available weight is not negative
            return availableWeight > 0 ? availableWeight : 0;
        }


        public static string GetInventoryDialogName(this InventoryModel inventory, IDataMemoryService dataMemoryService, bool show_weight = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}", inventory.Name);

            if (show_weight)
            {
                sb.AppendFormat(" ({0}g / {1}g)", inventory.GetInventoryItems().Sum(i => i.GetTotalWeight(dataMemoryService)), inventory.MaxWeight.ToString());
            }

            return sb.ToString();
        }

        #region GetInventoryItems
        public static List<InventoryItemModel> GetInventoryItems(this InventoryModel inventory)
        {
            using (var context = new DataContext())
            {
                return GetInventoryItems(inventory, context);
            }
        }

        public static List<InventoryItemModel> GetInventoryItems(this InventoryModel inventory, DataContext context)
        {
            if (inventory.Items != null)
            {
                return inventory.Items;
            }
            else
            {
                return context.Inventories.Find(inventory.Id).Items;
            }
        }
        #endregion
    }
}
