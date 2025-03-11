using OpenRP.Framework.Database.Models;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Entities
{
    public class BillsTransaction
    {
        public List<InventoryItemModel> BillsAdded { get; set; }
        public List<InventoryItemModel> BillsRemoved { get; set; }
        public decimal AmountRemaining { get; set; }
        public bool NotEnoughFunds { get; set; }
        public bool NotEnoughWeight { get; set; }
        public bool TransferNotPossible { get; set; }

        public BillsTransaction()
        {
            BillsAdded = new List<InventoryItemModel>();
            BillsRemoved = new List<InventoryItemModel>();
            AmountRemaining = 0;
            NotEnoughFunds = false;
            NotEnoughWeight = false;
            TransferNotPossible = false;
        }
    }
}
