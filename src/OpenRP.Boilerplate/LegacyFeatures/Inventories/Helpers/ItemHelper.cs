using OpenRP.Framework.Features.Items.Enums;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers
{
    public static class ItemHelper
    {
        public static bool IsItemWallet(this ItemModel item)
        {
            if (item.UseType == ItemType.Wallet)
            {
                return true;
            }
            return false;
        }
        public static bool IsItemSkin(this ItemModel item)
        {
            if (item.UseType == ItemType.Skin)
            {
                return true;
            }
            return false;
        }
        public static bool IsItemAttachment(this ItemModel item)
        {
            if (item.UseType == ItemType.Attachment)
            {
                return true;
            }
            return false;
        }

        public static bool IsItemCurrency(this ItemModel item)
        {
            if (item.UseType == ItemType.Currency)
            {
                return true;
            }
            return false;
        }

        public static bool IsItemVehicleKey(this ItemModel item)
        {
            if (item.UseType == ItemType.VehicleKey)
            {
                return true;
            }
            return false;
        }

        public static bool IsItemInventory(this ItemModel item)
        {
            return item.IsItemWallet();
        }

        public static CurrencyUnitModel GetItemCurrencyUnit(this ItemModel item, IDataMemoryService dataMemoryService)
        {
            if (item.IsItemCurrency())
            {
                ItemAdditionalData itemAdditionalData = ItemAdditionalData.Parse(item.UseValue);
                ulong? currencyUnit = itemAdditionalData.GetUlong("CURRENCY_UNIT");
                if (currencyUnit != null && currencyUnit.HasValue)
                {
                    return dataMemoryService.GetCurrencyUnits().FirstOrDefault(i => i.Id == currencyUnit.Value);
                }
            }
            return null;
        }
    }
}
