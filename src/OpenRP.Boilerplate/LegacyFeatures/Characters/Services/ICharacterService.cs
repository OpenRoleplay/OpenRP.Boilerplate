using SampSharp.Entities.SAMP;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Database.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Services
{
    public interface ICharacterService
    {
        CharacterPreferencesModel GetCharacterPreferences(Character character);
        void InvalidateCharacterPreferencesCache(ulong characterId);
        void UpdateCharacterPreferences(CharacterPreferencesModel characterPreferences);
        void OpenCharacterPreferencesDialog(Player player);
        void OpenCharacterTraitsDialog(Player player);
        bool SetCharacterDefaultWallet(Character character, InventoryItemModel inventoryItem);
        bool SetCharacterWearingInventorySkin(Character character, InventoryItemModel inventoryItem);
        InventoryModel GetCharacterWallet(Character character, IDataMemoryService dataMemoryService);
        List<InventoryItemModel> GetCharacterInventorySkins(Character character);
        List<InventoryItemModel> GetCharacterInventoryWallets(Character character);
        InventoryModel GetCharacterWallet(Character character, InventoryModel inventory, IDataMemoryService dataMemoryService);
        InventoryModel GetCharacterDefaultWallet(Character character, InventoryModel inventory, IDataMemoryService dataMemoryService);
    }
}
