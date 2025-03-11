using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Services;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Components;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Systems
{
    public class CurrencySystem : ISystem
    {
        [Timer(2500)]
        public void ProcessBillTransaction(IEntityManager entityManager, ICharacterService characterService, IInventoryService inventoryService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IChatService chatService, IDataMemoryService dataMemoryService)
        {
            foreach (BillTransactionBetweenPlayers billTransactionBetweenPlayers in entityManager.GetComponents<BillTransactionBetweenPlayers>())
            {
                if (billTransactionBetweenPlayers.Accepted)
                {
                    Player senderPlayer = billTransactionBetweenPlayers.sender;
                    Player receiverPlayer = billTransactionBetweenPlayers.receiver;

                    Character senderCharacter = senderPlayer.GetPlayerCurrentlyPlayingAsCharacter();
                    Character receiverCharacter = receiverPlayer.GetPlayerCurrentlyPlayingAsCharacter();

                    BillsTransaction billsTransaction = billTransactionBetweenPlayers.billsTransaction;

                    if (billTransactionBetweenPlayers.TransactionIndex >= billsTransaction.BillsRemoved.Count)
                    {
                        senderPlayer.DestroyComponents<BillTransactionBetweenPlayers>();
                        receiverPlayer.DestroyComponents<BillTransactionBetweenPlayers>();

                        senderPlayer.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "The transaction has completed.");
                        receiverPlayer.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "The transaction has completed.");

                        CharacterPreferencesModel senderPreferences = characterService.GetCharacterPreferences(senderCharacter);
                        CharacterPreferencesModel receiverPreferences = characterService.GetCharacterPreferences(receiverCharacter);

                        var eventArgsToSend = new OnPlayerCashUpdateEventArgs
                        {
                            Player = senderPlayer,
                            CurrencyId = senderPreferences.DefaultCurrencyId,
                        };
                        serverEventAggregator.PublishAsync(eventArgsToSend);

                        var eventArgsToSendTwo = new OnPlayerCashUpdateEventArgs
                        {
                            Player = receiverPlayer,
                            CurrencyId = receiverPreferences.DefaultCurrencyId,
                        };
                        serverEventAggregator.PublishAsync(eventArgsToSendTwo);

                        //senderPlayer.ToggleControllable(true);
                        //receiverPlayer.ToggleControllable(true);

                        senderPlayer.ClearAnimations();
                        receiverPlayer.ClearAnimations();
                        continue;
                    }

                    bool senderHasWallet = characterService.GetCharacterDefaultWallet(senderCharacter,
                        tempCharacterService.GetCharacterInventory(receiverCharacter), dataMemoryService) != null;

                    bool receiverHasWallet = characterService.GetCharacterDefaultWallet(receiverCharacter,
                        tempCharacterService.GetCharacterInventory(receiverCharacter), dataMemoryService) != null;

                    InventoryItemModel itemToRemove = billsTransaction.BillsRemoved[billTransactionBetweenPlayers.TransactionIndex];
                    InventoryItemModel itemToAdd = billsTransaction.BillsAdded[billTransactionBetweenPlayers.TransactionIndex];

                    if(billTransactionBetweenPlayers.ReceiverAction)
                    {
                        SendReceiveMessages(receiverPlayer, senderPlayer, itemToRemove, receiverHasWallet, chatService, dataMemoryService);
                        receiverPlayer.ApplyAnimation("BAR", "Barcustom_get", 4.1f, false, true, true, true, 0);

                        billTransactionBetweenPlayers.TransactionIndex++;
                    } else
                    {
                        SendTransferMessages(senderPlayer, receiverCharacter, itemToRemove, senderHasWallet, billTransactionBetweenPlayers, chatService, dataMemoryService);
                        senderPlayer.ApplyAnimation("DEALER", "shop_pay", 4.1f, false, true, true, true, 0);

                        inventoryService.RemoveItem(itemToRemove);
                        inventoryService.AddItem(itemToAdd);
                    }

                    billTransactionBetweenPlayers.ReceiverAction = !billTransactionBetweenPlayers.ReceiverAction;
                }
            }
        }

        private void SendTransferMessages(Player sender, Character receiverCharacter, InventoryItemModel bill, bool hasWallet, BillTransactionBetweenPlayers transaction, IChatService chatService, IDataMemoryService dataMemoryService)
        {
            string itemName = bill.GetItem(dataMemoryService).Name;
            string amountText = bill.Amount > 1 ? $"{bill.Amount} {itemName} bills" : $"a {itemName} bill";
            string meMessage = hasWallet
                ? $"reaches into their wallet and retrieves {amountText}, offering the bill to {receiverCharacter.GetName()}"
                : $"fumbles through their pockets before producing {amountText}, offering the bill to {receiverCharacter.GetName()}";

            if (transaction.TransactionIndex > 0)
            {
                meMessage += $" before repeating the process with other bills";
            }

            chatService.SendPlayerChatMessage(sender, PlayerChatMessageType.ME, $"{meMessage}.");
        }

        private void SendReceiveMessages(Player receiver, Player sender, InventoryItemModel bill, bool hasWallet, IChatService chatService, IDataMemoryService dataMemoryService)
        {
            string itemName = bill.GetItem(dataMemoryService).Name;

            string meMessage = hasWallet
                ? $"carefully examines each {itemName} bill before filing them in their wallet's denomination slots"
                : $"palms the {itemName} bill{(bill.Amount > 1 ? "s" : "")} smoothly into their sleeve lining";

            chatService.SendPlayerChatMessage(receiver, PlayerChatMessageType.ME, $"{meMessage}.");
        }
    }
}
