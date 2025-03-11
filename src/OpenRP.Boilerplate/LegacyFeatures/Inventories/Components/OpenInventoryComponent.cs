using OpenRP.Framework.Database.Models;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Components
{
    public class OpenInventoryComponent : Component
    {
        public InventoryModel openedInventory { get; set; }
        public List<InventoryItemModel> openedInventoryItems { get; set; }
        public InventoryItemModel selectedInventoryItem { get; set; }
        public List<string> actionsList { get; set; }

        public OpenInventoryComponent()
        {
            openedInventory = null;
            openedInventoryItems = null;
            selectedInventoryItem = null;
            actionsList = null;
        }
    }
}
