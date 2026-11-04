using Microsoft.EntityFrameworkCore;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRP.Boilerplate.LegacyFeatures.Inventories.Services
{
    public class CharacterInventoryService : ICharacterInventoryService
    {
        private const string DefaultWalletKey = "DEFAULT_WALLET";
        private const string WearingKey = "WEARING";
        private const string SkinKey = "SKIN";

        private readonly DataContext _dataContext;
        private readonly IDataMemoryService _dataMemoryService;

        public CharacterInventoryService(DataContext dataContext, IDataMemoryService dataMemoryService)
        {
            _dataContext = dataContext;
            _dataMemoryService = dataMemoryService;
        }

        /// <summary>
        /// The character's inventory with its items. The Framework CharacterService makes sure every loaded character has one.
        /// </summary>
        public InventoryModel GetCharacterInventoryModel(Character character)
        {
            return _dataContext.Characters
                .Include(c => c.Inventory)
                .ThenInclude(i => i.Items)
                .FirstOrDefault(c => c.Id == character.GetId())
                ?.Inventory;
        }

        public List<InventoryItemModel> GetCharacterInventorySkins(Character character)
        {
            return GetCharacterInventoryModel(character).GetInventoryItems().Where(i => i.GetItem(_dataMemoryService).IsItemSkin()).ToList();
        }

        public List<InventoryItemModel> GetCharacterInventoryWallets(Character character)
        {
            return GetCharacterInventoryWallets(GetCharacterInventoryModel(character));
        }

        public List<InventoryItemModel> GetCharacterInventoryWallets(InventoryModel inventory)
        {
            return inventory.GetInventoryItems().Where(i => i.GetItem(_dataMemoryService).IsItemWallet()).ToList();
        }

        public InventoryModel GetCharacterWallet(Character character, IDataMemoryService dataMemoryService)
        {
            return GetCharacterWallet(character, GetCharacterInventoryModel(character), dataMemoryService);
        }

        /// <summary>
        /// The default wallet if one is set, otherwise the first wallet, otherwise the character inventory itself.
        /// </summary>
        public InventoryModel GetCharacterWallet(Character character, InventoryModel inventory, IDataMemoryService dataMemoryService)
        {
            return GetCharacterDefaultWallet(inventory, dataMemoryService)
                ?? GetCharacterInventoryWallets(inventory).FirstOrDefault()?.GetItemInventory(dataMemoryService)
                ?? GetCharacterInventoryModel(character);
        }

        public InventoryModel GetCharacterDefaultWallet(InventoryModel inventory, IDataMemoryService dataMemoryService)
        {
            return GetCharacterInventoryWallets(inventory)
                .FirstOrDefault(i => ItemAdditionalData.Parse(i.AdditionalData).GetBoolean(DefaultWalletKey) == true)
                ?.GetItemInventory(dataMemoryService);
        }

        public bool SetCharacterDefaultWallet(Character character, InventoryItemModel inventoryItem)
        {
            try
            {
                foreach (InventoryItemModel wallet in GetCharacterInventoryWallets(character))
                {
                    SetItemFlag(wallet, DefaultWalletKey, false);
                }

                SetItemFlag(inventoryItem, DefaultWalletKey, true);
                _dataContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Marks the skin item as worn and puts the skin on the character. The character's skin
        /// is saved by the Framework character persistence system, not here.
        /// </summary>
        public bool SetCharacterWearingInventorySkin(Character character, InventoryItemModel inventoryItem)
        {
            try
            {
                int? newSkin = ItemAdditionalData.Parse(inventoryItem.AdditionalData).GetInt(SkinKey);
                if (newSkin == null)
                {
                    return false;
                }

                foreach (InventoryItemModel skin in GetCharacterInventorySkins(character))
                {
                    SetItemFlag(skin, WearingKey, false);
                }

                SetItemFlag(inventoryItem, WearingKey, true);
                _dataContext.SaveChanges();

                character.SetSkin(newSkin.Value);
                character.GetPlayer().Skin = newSkin.Value;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Changes one boolean flag in the item's additional data. Saved on the next SaveChanges.
        /// </summary>
        private void SetItemFlag(InventoryItemModel inventoryItem, string key, bool value)
        {
            ItemAdditionalData additionalData = ItemAdditionalData.Parse(inventoryItem.AdditionalData);
            if (additionalData.GetBoolean(key) == value)
            {
                return;
            }

            additionalData.SetBoolean(key, value);
            InventoryItemModel inventoryItemToUpdate = _dataContext.InventoryItems.Find(inventoryItem.Id);
            inventoryItemToUpdate.AdditionalData = additionalData.ToString();
        }
    }
}
