using System.Collections.Generic;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Services
{
    /// <summary>
    /// Legacy wallet and skin logic on top of InventoryModel. To be replaced by the Framework
    /// Inventories feature when Inventories and Currencies are ported.
    /// </summary>
    public interface ICharacterInventoryService
    {
        InventoryModel GetCharacterInventoryModel(Character character);
        List<InventoryItemModel> GetCharacterInventorySkins(Character character);
        List<InventoryItemModel> GetCharacterInventoryWallets(Character character);
        List<InventoryItemModel> GetCharacterInventoryWallets(InventoryModel inventory);
        InventoryModel GetCharacterWallet(Character character, IDataMemoryService dataMemoryService);
        InventoryModel GetCharacterWallet(Character character, InventoryModel inventory, IDataMemoryService dataMemoryService);
        InventoryModel GetCharacterDefaultWallet(InventoryModel inventory, IDataMemoryService dataMemoryService);
        bool SetCharacterDefaultWallet(Character character, InventoryItemModel inventoryItem);
        bool SetCharacterWearingInventorySkin(Character character, InventoryItemModel inventoryItem);
    }
}
