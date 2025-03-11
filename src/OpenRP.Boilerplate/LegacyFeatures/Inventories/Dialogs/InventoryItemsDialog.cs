using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Chat.Services;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Components;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.ColAndreas.Entities.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using System.Text;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Dialogs
{
    public class InventoryItemsDialog
    {
        public static void Open(Player player, List<InventoryItemModel> inventoryItemsToShow, IDialogService dialogService, IEntityManager entityManager, IColAndreasService colAndreasService, ICharacterService characterService, IDroppedItemService droppedItemService, IInventoryService inventoryService, IChatService chatService, IDataMemoryService dataMemoryService, InventoryArgument[] args = null)
        {
            Character characterComponent = player.GetComponent<Character>();
            if (characterComponent != null)
            {
                OpenInventoryComponent openInventoryComponent = player.GetComponent<OpenInventoryComponent>();

                if(openInventoryComponent != null && openInventoryComponent.openedInventory != null)
                {
                    if(inventoryItemsToShow == null)
                    {
                        inventoryItemsToShow = new List<InventoryItemModel>();
                    }

                    StringBuilder inventory_name_builder = new StringBuilder();

                    InventoryModel parentInventory = openInventoryComponent.openedInventory.GetParentInventory(dataMemoryService);
                    if (parentInventory != null)
                    {
                        inventory_name_builder.Append(parentInventory.GetInventoryDialogName(dataMemoryService, false));
                        inventory_name_builder.Append(ChatColor.CornflowerBlue + " -> " + ChatColor.White);
                    }
                    inventory_name_builder.Append(openInventoryComponent.openedInventory.GetInventoryDialogName(dataMemoryService));

                    string inventory_name = DialogHelper.GetTitle(inventory_name_builder.ToString());

                    // Fill Headers
                    List<string> inventoryColumnHeaders = new List<string>();

                    inventoryColumnHeaders.Add(ChatColor.CornflowerBlue + "ItemModel");
                    inventoryColumnHeaders.Add(ChatColor.CornflowerBlue + "Amount");

                    if (args == null || !args.Contains(InventoryArgument.HideTotalWeight))
                    {
                        inventoryColumnHeaders.Add(ChatColor.CornflowerBlue + "Total Weight");
                    }

                    if (args == null || !args.Contains(InventoryArgument.HideExtraInformation))
                    {
                        inventoryColumnHeaders.Add(ChatColor.CornflowerBlue + "Extra Information");
                    }

                    TablistDialog inventory = new TablistDialog(inventory_name, "Details", "Cancel", inventoryColumnHeaders.ToArray());

                    foreach (InventoryItemModel inventoryItem in inventoryItemsToShow)
                    {
                        List<string> extra_information = new List<string>();

                        if (!String.IsNullOrEmpty(inventoryItem.AdditionalData))
                        {
                            ItemAdditionalData ad = ItemAdditionalData.Parse(inventoryItem.AdditionalData);

                            if (inventoryItem.GetItem(dataMemoryService).IsItemSkin() && ad.GetBoolean("WEARING") != null & ad.GetBoolean("WEARING") == true)
                            {
                                extra_information.Add("Wearing");
                            }

                            if (inventoryItem.GetItem(dataMemoryService).IsItemAttachment() && ad.GetBoolean("WEARING") != null & ad.GetBoolean("WEARING") == true)
                            {
                                extra_information.Add("Wearing");
                            }
                        }

                        if (inventoryItem.UsesRemaining != null)
                        {
                            extra_information.Add($"{inventoryItem.UsesRemaining}/{inventoryItem.GetItem(dataMemoryService).MaxUses} Durability");
                        }

                        // Fill Columns
                        List<string> inventoryColumns = new List<string>();

                        inventoryColumns.Add(inventoryItem.GetName(dataMemoryService));
                        inventoryColumns.Add(inventoryItem.Amount.ToString());

                        if (args == null || !args.Contains(InventoryArgument.HideTotalWeight))
                        {
                            inventoryColumns.Add(String.Format("{0}g", inventoryItem.GetTotalWeight(dataMemoryService)));
                        }

                        if (args == null || !args.Contains(InventoryArgument.HideExtraInformation))
                        {
                            inventoryColumns.Add(String.Join(ChatColor.CornflowerBlue + " | " + ChatColor.White, extra_information));
                        }

                        inventory.Add(inventoryColumns.ToArray());
                    }

                    void InventoryItemsDialogHandler(TablistDialogResponse r)
                    {
                        if (r.Response == DialogResponse.LeftButton)
                        {
                            openInventoryComponent.selectedInventoryItem = openInventoryComponent.openedInventoryItems.ElementAt(r.ItemIndex);

                            InventoryItemSelectedDialog.Open(player, openInventoryComponent, dialogService, entityManager, colAndreasService, characterService, droppedItemService, inventoryService, chatService, dataMemoryService);
                        }
                        else
                        {
                            player.DestroyComponents<OpenInventoryComponent>();
                        }
                    }

                    dialogService.Show(player, inventory, InventoryItemsDialogHandler);
                }
            }
        }
    }
}
