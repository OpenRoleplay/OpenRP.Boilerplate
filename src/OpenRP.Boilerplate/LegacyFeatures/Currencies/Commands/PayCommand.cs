using OpenRP.Framework.Database.Models;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Helpers;
using OpenRP.Framework.Features.Commands.Attributes;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Components;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Components;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Database.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Commands
{
    public class PayCommand : ISystem
    {
        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Pay money to another player.",
            CommandGroups = new string[] { "Currencies"} )]
        public async void Pay(Player player, Player playerid, ICurrencyService currencyService, IDataMemoryService dataMemoryService, string currency_code, int amount)
        {
            if(player == playerid)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You can't transfer money to yourself!");
                return;
            }

            if(!player.IsInRangeOfPoint(3.0f, playerid.Position))
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You must be near the player in order to pay them!");
                return;
            }

            CurrencyModel currencyModel = dataMemoryService.GetCurrencies().FirstOrDefault(i => i.CurrencyCode.ToUpper() == currency_code.ToUpper());
            if (currencyModel == null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You must enter a valid currency code!");
                return;
            }

            BillsTransaction billsTransaction = await currencyService.PrepareBillTransactionBetweenPlayers(player, playerid, currencyModel.Id, amount);
            if(billsTransaction.NotEnoughWeight || billsTransaction.TransferNotPossible)
            {
                return;
            }

            if(player.GetComponent<OpenInventoryComponent>() != null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You currently have your inventory open!");
                return;
            }

            if (playerid.GetComponent<OpenInventoryComponent>() != null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "The recipient has their inventory open!");
                return;
            }

            Character senderCharacter = player.GetPlayerCurrentlyPlayingAsCharacter();
            Character receiverCharacter = playerid.GetPlayerCurrentlyPlayingAsCharacter();

            BillTransactionBetweenPlayers billTransactionSender = player.AddComponent<BillTransactionBetweenPlayers>();
            billTransactionSender.sender = player;
            billTransactionSender.receiver = playerid;
            billTransactionSender.amount = amount;
            billTransactionSender.currencyCode = currency_code;
            billTransactionSender.billsTransaction = billsTransaction;

            BillTransactionBetweenPlayers billTransactionRecipient = playerid.AddComponent<BillTransactionBetweenPlayers>();
            billTransactionRecipient.sender = player;
            billTransactionRecipient.receiver = playerid;
            billTransactionRecipient.amount = amount;
            billTransactionRecipient.currencyCode = currency_code;
            billTransactionRecipient.billsTransaction = billsTransaction;

            player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"Sent a request to {receiverCharacter.GetCharacterModel().GetCharacterName()} to start a transaction.");
            playerid.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"{senderCharacter.GetCharacterModel().GetCharacterName()} has sent a request to start a transaction to transfer {currencyService.GetCurrencyFormat(currencyModel.Id, amount)} to you. Type /acceptpay to proceed.");
        }

        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Pay money to another player.",
            CommandGroups = new string[] { "Currencies" })]
        public void Pay(Player player, IDataMemoryService dataMemoryService)
        {
            player.SendPlayerInfoMessage(PlayerInfoMessageType.SYNTAX, "/pay [playerid] [currency code] [amount]");
            player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "Currency codes:");
            foreach (CurrencyModel currency in dataMemoryService.GetCurrencies())
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"{ChatColor.Highlight}{currency.CurrencyCode}{ChatColor.White} = {currency.Name}");
            }
        }
    }
}
