using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Shared.ServerEvents.Attributes;
using OpenRP.Framework.Shared.ServerEvents.Entities;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Framework.Features.Accounts.Components;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Services;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Systems
{
    public class CurrencyServerSystem : IServerSystem
    {
        [ServerEvent]
        public void OnPlayerCashUpdate(OnPlayerCashUpdateEventArgs args, ICurrencyService currencyService)
        {
            try
            {
                Character character = args.Player.GetComponent<Character>();
                var accountComponent = args.Player.GetComponent<Account>();

                if (accountComponent == null)
                    return;

                if (!args.Player.IsPlayerPlayingAsCharacter())
                    return;

                if (character.GetPreferences().GetDefaultCurrencyId() == args.CurrencyId)
                {
                    decimal newAmount = currencyService.GetCharacterCurrency(character, args.CurrencyId);

                    args.Player.ResetMoney();
                    args.Player.GiveMoney(Convert.ToInt32(Math.Floor(newAmount)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [ServerEvent]
        public async void OnCharacterSelected(OnCharacterSelectedEventArgs eventArgs, IServerEventAggregator serverEventAggregator)
        {
            Character character = eventArgs.Character;

            var eventArgsToSend = new OnPlayerCashUpdateEventArgs
            {
                Player = eventArgs.Player,
                CurrencyId = character.GetPreferences().GetDefaultCurrencyId(),
            };
            await serverEventAggregator.PublishAsync(eventArgsToSend);
        }
    }
}
