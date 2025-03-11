using OpenRP.Boilerplate.LegacyFeatures.Currencies.Entities;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Components
{
    public class BillTransactionBetweenPlayers : Component
    {
        public Player sender { get; set; }
        public Player receiver { get; set; }
        public string currencyCode { get; set; }
        public decimal amount { get; set; }
        public BillsTransaction billsTransaction { get; set; }
        public bool ReceiverAction { get; set; }
        public bool Accepted { get; set; }
        public int TransactionIndex { get; set; }
    }
}
