using EFCoreSecondLevelCacheInterceptor;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Dialogs.CharacterRoleplayConsent;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Dialogs.CharacterTraits;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Services
{
    public class CharacterService : ICharacterService
    {
        private IDialogService _dialogService;
        private readonly IEFCacheServiceProvider _efCacheServiceProvider;
        private IEntityManager _entityManager;
        private DataContext _dataContext;
        private IInventoryService _inventoryService;
        private ITempCharacterService _tempCharacterService;
        private IDataMemoryService _dataMemoryService;
        public CharacterService(IDialogService dialogService, IEFCacheServiceProvider efCacheServiceProvider, DataContext dataContext, IEntityManager entityManager, IInventoryService inventoryService, ITempCharacterService tempCharacterService, IDataMemoryService dataMemoryService)
        {
            _dialogService = dialogService;
            _efCacheServiceProvider = efCacheServiceProvider;
            _dataContext = dataContext;
            _entityManager = entityManager;
            _inventoryService = inventoryService;
            _tempCharacterService = tempCharacterService;
            _dataMemoryService = dataMemoryService;
        }

        public CharacterPreferencesModel GetCharacterPreferences(Character character)
        {
            if (character != null)
            {
                CharacterPreferencesModel preferences = _dataContext.CharacterPreferences
                    .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(10), $"CharacterPreferences_{character.GetDatabaseId()}")
                    .FirstOrDefault(i => i.CharacterId == character.GetDatabaseId());

                if (preferences == null)
                {
                    preferences = new CharacterPreferencesModel();
                    preferences.CharacterId = character.GetDatabaseId();

                    _dataContext.CharacterPreferences.Add(preferences);
                    _dataContext.SaveChanges();
                }

                return preferences;
            }
            return null;
        }

        public void InvalidateCharacterPreferencesCache(ulong characterId)
        {
            string cacheKey = $"CharacterPreferences_{characterId}";

            // Create a HashSet containing the cacheKey
            ISet<string> cacheKeys = new HashSet<string> { cacheKey };

            // Instantiate EFCacheKey with the HashSet
            EFCacheKey efCacheKey = new EFCacheKey(cacheKeys);

            // Invalidate the specific cache entry
            _efCacheServiceProvider.InvalidateCacheDependencies(efCacheKey);

            Console.WriteLine($"Cache invalidated for key: {cacheKey}");
        }

        public void UpdateCharacterPreferences(CharacterPreferencesModel characterPreferences)
        {
            if (characterPreferences != null)
            {
                // Save changes to the database
                _dataContext.CharacterPreferences.Update(characterPreferences);
                _dataContext.SaveChanges();

                // Invalidate the cache to ensure fresh data next time
                InvalidateCharacterPreferencesCache(characterPreferences.CharacterId);
            }
        }

        public void OpenCharacterPreferencesDialog(Player player)
        {
            CharacterPreferencesDialog.Open(player, _dialogService, this);
        }

        public void OpenCharacterTraitsDialog(Player player)
        {
            CharacterTraitsDialog.Open(player, _dialogService, this);
        }

        public bool SetCharacterDefaultWallet(Character character, InventoryItemModel inventoryItem)
        {
            try
            {
                using (var context = new DataContext())
                {
                    foreach (InventoryItemModel wallet in GetCharacterInventoryWallets(character))
                    {
                        ItemAdditionalData itemAdditionalData = ItemAdditionalData.Parse(wallet.AdditionalData);

                        if (itemAdditionalData.GetBoolean("DEFAULT_WALLET") != null && itemAdditionalData.GetBoolean("DEFAULT_WALLET") == true)
                        {
                            itemAdditionalData.SetBoolean("DEFAULT_WALLET", false);

                            InventoryItemModel inventoryItemToUpdate = context.InventoryItems.Find(wallet.Id);
                            inventoryItemToUpdate.AdditionalData = itemAdditionalData.ToString();
                        }
                    }

                    ItemAdditionalData itemAdditionalDataNewSkin = ItemAdditionalData.Parse(inventoryItem.AdditionalData);

                    itemAdditionalDataNewSkin.SetBoolean("DEFAULT_WALLET", true);

                    InventoryItemModel inventoryItemToUpdateNewSkin = context.InventoryItems.Find(inventoryItem.Id);
                    inventoryItemToUpdateNewSkin.AdditionalData = itemAdditionalDataNewSkin.ToString();
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public bool SetCharacterWearingInventorySkin(Character character, InventoryItemModel inventoryItem)
        {
            try
            {
                foreach (InventoryItemModel skin in GetCharacterInventorySkins(character))
                {
                    ItemAdditionalData itemAdditionalData = ItemAdditionalData.Parse(skin.AdditionalData);

                    if (itemAdditionalData.GetBoolean("WEARING") != null && itemAdditionalData.GetBoolean("WEARING") == true)
                    {
                        itemAdditionalData.SetBoolean("WEARING", false);

                        InventoryItemModel inventoryItemToUpdate = _dataContext.InventoryItems.Find(skin.Id);
                        inventoryItemToUpdate.AdditionalData = itemAdditionalData.ToString();
                    }
                }

                ItemAdditionalData itemAdditionalDataNewSkin = ItemAdditionalData.Parse(inventoryItem.AdditionalData);

                itemAdditionalDataNewSkin.SetBoolean("WEARING", true);

                InventoryItemModel inventoryItemToUpdateNewSkin = _dataContext.InventoryItems.Find(inventoryItem.Id);
                inventoryItemToUpdateNewSkin.AdditionalData = itemAdditionalDataNewSkin.ToString();

                int? newSkin = itemAdditionalDataNewSkin.GetInt("SKIN");
                if (newSkin != null)
                {
                    CharacterModel characterToUpdate = _dataContext.Characters.Find(character.GetDatabaseId());
                    characterToUpdate.Skin = character.GetPlayer().Skin = newSkin.Value;

                    _dataContext.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public InventoryModel GetCharacterWallet(Character character, IDataMemoryService dataMemoryService)
        {
            InventoryModel inventoryModel = _tempCharacterService.GetCharacterInventory(character);
            return GetCharacterWallet(character, inventoryModel, dataMemoryService);
        }

        public List<InventoryItemModel> GetCharacterInventorySkins(Character character)
        {
            return _tempCharacterService.GetCharacterInventory(character).GetInventoryItems().Where(i => i.GetItem(_dataMemoryService).IsItemSkin()).ToList();
        }

        public List<InventoryItemModel> GetCharacterInventoryWallets(Character character)
        {
            return GetCharacterInventoryWallets(character, _tempCharacterService.GetCharacterInventory(character));
        }

        public List<InventoryItemModel> GetCharacterInventoryWallets(Character character, InventoryModel inventory)
        {
            return inventory.GetInventoryItems().Where(i => i.GetItem(_dataMemoryService).IsItemWallet()).ToList();
        }

        public InventoryModel GetCharacterDefaultWallet(Character character, InventoryModel inventory, IDataMemoryService dataMemoryService)
        {
            return GetCharacterInventoryWallets(character, inventory).FirstOrDefault(i => ItemAdditionalData.Parse(i.AdditionalData).GetBoolean("DEFAULT_WALLET") == true)?.GetItemInventory(dataMemoryService);
        }

        public InventoryModel GetCharacterWallet(Character character, InventoryModel inventory, IDataMemoryService dataMemoryService)
        {
            // Get the default wallet if one is set, if not, get the first wallet in the inventory.
            InventoryModel wallet = GetCharacterDefaultWallet(character, inventory, dataMemoryService);

            // If no default wallet is set, get the first wallet in the inventory.
            if (wallet == null)
            {
                wallet = GetCharacterInventoryWallets(character, inventory).FirstOrDefault()?.GetItemInventory(dataMemoryService);
            }

            // If no wallet, then simply use the character inventory.
            if (wallet == null)
            {
                wallet = _tempCharacterService.GetCharacterInventory(character);
            }

            return wallet;
        }
    }
}
