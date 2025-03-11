using OpenRP.Framework.Database.Models;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Services
{
    public interface IInventoryService
    {
        InventoryItemModel AddItem(InventoryItemModel item);
        InventoryItemModel AddItem(ulong inventoryId, ItemModel item, uint amount);
        InventoryItemModel PrepareItem(ulong inventoryId, ItemModel item, uint amount);
        void RemoveItem(InventoryItemModel item, uint amountToRemove);
        void RemoveItem(InventoryItemModel item);
        bool UseItem(Player player, InventoryItemModel item);
        InventoryItemModel HasItem(InventoryItemModel item);
        InventoryItemModel PrepareItem(ulong inventoryId, InventoryItemModel item, uint amount);
    }
}
