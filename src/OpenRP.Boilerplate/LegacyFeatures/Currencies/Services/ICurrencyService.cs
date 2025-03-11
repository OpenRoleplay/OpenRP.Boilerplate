using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Entities;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Services
{
    public interface ICurrencyService
    {
        Task<BillsTransaction> GivePlayerCurrency(Player player, uint currencyId, decimal amount);
        string GetCurrencyFormat(ulong currencyId, decimal amount);
        decimal GetCharacterCurrency(Character character, ulong currencyId);
        Task<BillsTransaction> PrepareBillTransactionBetweenPlayers(Player sourcePlayer, Player destPlayer, ulong currencyId, decimal amount);
    }
}
