using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Vehicles.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.Vehicles.Managers
{
    public class CharacterVehicleManager : ICharacterVehicleManager
    {
        private readonly DataContext _DataContext;
        private readonly ITempCharacterService _characterService;
        private readonly IDataMemoryService _dataMemoryService;
        public CharacterVehicleManager(DataContext dataContext, ITempCharacterService characterService, IDataMemoryService dataMemoryService) 
        { 
            _DataContext = dataContext;
            _characterService = characterService;
            _dataMemoryService = dataMemoryService;
        }

        public bool HasVehicleKey(Character character, ulong vehicleId)
        {
            // Get the player's inventory
            var inventoryItems = _characterService.GetCharacterInventory(character).GetInventoryItems();

            // Check each item in the inventory for a car key matching the car ID
            foreach (var item in inventoryItems)
            {
                // Skip if the item is null or doesn't have additional data
                if (item == null || !item.GetItem(_dataMemoryService).IsItemVehicleKey() || string.IsNullOrEmpty(item.AdditionalData))
                    continue;

                // Parse the additional data to check if it's a car key and matches the car ID
                ItemAdditionalData itemData = ItemAdditionalData.Parse(item.AdditionalData);

                if (ulong.TryParse(itemData.GetString("VEHICLE_ID"), out ulong keyVehicleId) &&
                    keyVehicleId == vehicleId)
                {
                    return true; // The player has the key for this car
                }
            }

            // No matching key found
            return false;
        }
    }
}
