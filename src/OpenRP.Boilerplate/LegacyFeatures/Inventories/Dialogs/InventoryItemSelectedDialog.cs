using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Services;
using OpenRP.Framework.Shared.Dialogs;
using OpenRP.Framework.Shared.Dialogs.Enums;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Components;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.ColAndreas.Entities.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Shared.Dialogs.Extensions;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Dialogs
{
    public class InventoryItemSelectedDialog
    {
        public static void Open(Player player, OpenInventoryComponent openInventoryComponent, IDialogService dialogService, IEntityManager entityManager, IColAndreasService colAndreasService, ICharacterService characterService, IDroppedItemService droppedItemService, IInventoryService inventoryService, IChatService chatService, IDataMemoryService dataMemoryService)
        {
            ListDialog listDialog = new ListDialog(DialogHelper.GetTitle(openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).Name), "Select", "Cancel");

            List<string> listItems = new List<string>();

            if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemWallet())
            {
                listItems.Add("Open");
                listItems.Add("Set As Default");
            }
            else if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemSkin()
                          || openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemAttachment())
            {
                listItems.Add("Wear");
            }
            else
            {
                listItems.Add("Use");
            }
            if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemAttachment())
            {
                listItems.Add("Edit");
            }
            listItems.Add("Description");
            if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).CanDestroy)
            {
                listItems.Add("Destroy");
            }
            if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).CanDrop)
            {
                listItems.Add("Drop");
            }
            listItems.Sort();

            listItems.ForEach(item => { 
                listDialog.Add(item); 
            });
            openInventoryComponent.actionsList = listItems;

            void InventoryItemSelectedItemActionsDialogHandler(ListDialogResponse r)
            {
                if (r.Response == DialogResponse.LeftButton)
                {
                    using (var context = new DataContext())
                    {
                        Character character = player.GetPlayerCurrentlyPlayingAsCharacter();

                        switch (openInventoryComponent.actionsList[r.ItemIndex])
                        {
                            case "Use":
                                inventoryService.UseItem(player, openInventoryComponent.selectedInventoryItem);
                                break;
                            case "Wear":
                                if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemSkin())
                                {
                                    if (characterService.SetCharacterWearingInventorySkin(character, openInventoryComponent.selectedInventoryItem))
                                    {
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "changed their clothes.");
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "You have succesfully changed your clothes.");
                                    }
                                    else
                                    {
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "An unknown problem occured and we could not change your clothes.");
                                    }
                                }
                                else if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemAttachment())
                                {
                                    if (characterService.SetCharacterWearingInventorySkin(character, openInventoryComponent.selectedInventoryItem))
                                    {
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "changed their clothes.");
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "You have succesfully changed your clothes.");
                                    }
                                    else
                                    {
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "An unknown problem occured and we could not change your clothes.");
                                    }
                                }
                                break;
                            case "Open":
                                InventoryModel itemInventory = openInventoryComponent.selectedInventoryItem.GetItemInventory(dataMemoryService);
                                InventoryDialog.Open(player, itemInventory, dialogService, entityManager, colAndreasService, characterService, droppedItemService, inventoryService, chatService, dataMemoryService);
                                break;
                            case "Description":
                                string description = String.Empty;
                                if (String.IsNullOrEmpty(openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).Description))
                                {
                                    description = "This item does not have a description.";
                                }
                                else
                                {
                                    description = openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).Description.InsertNewlinesAtLength(100);
                                }

                                MessageDialog messageDialog = new MessageDialog(DialogHelper.GetTitle(openInventoryComponent.selectedInventoryItem.GetName(dataMemoryService), "Description"), description, "Close");
                                dialogService.Show(player, messageDialog);
                                break;
                            case "Set As Default":
                                if (openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).IsItemWallet()) // Wallets
                                {
                                    characterService.SetCharacterDefaultWallet(character, openInventoryComponent.selectedInventoryItem);
                                }
                                break;
                            case "Drop":
                                BetterInputDialog dropAmountDialog = new BetterInputDialog("Drop", "Go Back");
                                dropAmountDialog.SetTitle(TitleType.Children, openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).Name, "Drop");
                                dropAmountDialog.SetContent($"You can enter the amount of the item you want to drop here.\r\n\r\n{ChatColor.CornflowerBlue}Note:{ChatColor.White} If you want to drop all items, just type in the maximum amount ({ChatColor.Highlight}{openInventoryComponent.selectedInventoryItem.Amount}{ChatColor.White}) you have.");

                                void DropAmountDialog(InputDialogResponse r)
                                {
                                    if(r.Response == DialogResponse.RightButtonOrCancel)
                                    {
                                        dialogService.Show(player, listDialog, InventoryItemSelectedItemActionsDialogHandler);
                                    }

                                    if(int.TryParse(r.InputText, out int amount))
                                    {
                                        if(amount > 0)
                                        {
                                            bool success = droppedItemService.DropItem(player, openInventoryComponent.selectedInventoryItem, amount);
                                            if (success)
                                            {
                                                return;
                                            }
                                        }
                                    }

                                    BetterMessageDialog amountNotCorrect = new BetterMessageDialog("Ok");
                                    amountNotCorrect.SetTitle(TitleType.Children, openInventoryComponent.selectedInventoryItem.GetItem(dataMemoryService).Name, "Drop", "Invalid Drop Amount");
                                    amountNotCorrect.SetContent($"{ChatColor.Highlight}The drop item amount you provided is invalid!{ChatColor.White} Please ensure you enter a positive number that doesn't exceed the amount of items in your inventory.");

                                    void DropAmountInvalid(MessageDialogResponse r)
                                    {
                                        dialogService.Show(player, dropAmountDialog, DropAmountDialog);
                                    }

                                    dialogService.Show(player, amountNotCorrect, DropAmountInvalid);
                                }

                                dialogService.Show(player, dropAmountDialog, DropAmountDialog);
                                break;
                        }
                    }
                }

                player.DestroyComponents<OpenInventoryComponent>();
            }

            dialogService.Show(player, listDialog, InventoryItemSelectedItemActionsDialogHandler);
        }
    }
}
