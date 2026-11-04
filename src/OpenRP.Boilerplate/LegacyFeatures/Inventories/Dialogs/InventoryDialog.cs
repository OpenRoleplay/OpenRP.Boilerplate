using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Shared.Chat.Services;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Components;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.ColAndreas.Entities.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Dialogs
{
    public static class InventoryDialog
    {
        public static void Open(Player player, InventoryModel inventory, IDialogService dialogService, IEntityManager entityManager, IColAndreasService colAndreasService, ICharacterInventoryService characterInventoryService, IDroppedItemService droppedItemService, IInventoryService inventoryService, IChatService chatService, IDataMemoryService dataMemoryService, InventoryArgument[] args = null)
        {
            OpenInventoryComponent openInventoryComponent = player.AddComponent<OpenInventoryComponent>();

            openInventoryComponent.openedInventory = inventory;
            openInventoryComponent.openedInventoryItems = openInventoryComponent.openedInventory.GetInventoryItems().OrderByDescending(i => i.GetTotalWeight(dataMemoryService)).ToList();

            InventoryItemsDialog.Open(player, openInventoryComponent.openedInventoryItems, dialogService, entityManager, colAndreasService, characterInventoryService, droppedItemService, inventoryService, chatService, dataMemoryService, args);
        }
    }
}
