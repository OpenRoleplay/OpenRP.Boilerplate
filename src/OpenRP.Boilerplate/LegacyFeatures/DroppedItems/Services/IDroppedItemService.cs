using OpenRP.Framework.Database.Models;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Components;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services
{
    public interface IDroppedItemService
    {
        void LoadDroppedItems();
        bool DropItem(Player player, InventoryItemModel inventoryItem, int? dropQuantity = null, int spawnInRange = 0);
        IEnumerable<DroppedItem> GetAllDroppedItems();
    }
}
