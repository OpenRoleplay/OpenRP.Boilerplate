using OpenRP.Boilerplate.LegacyFeatures.Characters.Helpers;
using OpenRP.Framework.Features.Commands.Attributes;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Components;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Components;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Features.Players.Extensions;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Commands
{
    public class AcPayCommand : ISystem
    {
        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Accept a transaction request from another player.",
            CommandGroups = new string[] { "Currencies"} )]
        public async void AcceptPay(Player player)
        {
            BillTransactionBetweenPlayers billTransaction = player.GetComponent<BillTransactionBetweenPlayers>();

            if(billTransaction == null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You currently do not have a pending transaction request!");
                return;
            }

            if(!player.IsInRangeOfPoint(3.0f, billTransaction.sender.Position))
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You must be near the player in order to accept the transaction!");
                return;
            }

            if (player.GetComponent<OpenInventoryComponent>() != null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You currently have your inventory open!");
                return;
            }

            if (billTransaction.sender.GetComponent<OpenInventoryComponent>() != null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "The sender has their inventory open!");
                return;
            }

            Character receiverCharacter = player.GetPlayerCurrentlyPlayingAsCharacter();

            player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "You have accepted the transaction. It will now start.");
            billTransaction.sender.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, $"{receiverCharacter.GetCharacterModel().GetCharacterName()} has accepted the transaction. It will now start.");

            //billTransaction.receiver.ToggleControllable(false);
            //billTransaction.sender.ToggleControllable(false);

            billTransaction.Accepted = true;
        }
    }
}
