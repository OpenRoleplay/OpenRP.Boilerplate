using OpenRP.Boilerplate.Data;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Inventories.Components;
using OpenRP.Framework.Features.Vehicles.Services;
using System.Linq;

namespace OpenRP.Boilerplate.LegacyFeatures.Vehicles.Managers
{
    public class CharacterVehicleManager : ICharacterVehicleManager
    {
        private const string VehicleIdKey = "VEHICLE_ID";

        private readonly DataContext _DataContext;
        public CharacterVehicleManager(DataContext dataContext)
        {
            _DataContext = dataContext;
        }

        public bool HasVehicleKey(Character character, ulong vehicleId)
        {
            Inventory? inventory = character.GetInventory();
            if (inventory == null)
            {
                return false;
            }

            return inventory.GetInventoryItems().Any(inventoryItem => IsKeyForVehicle(inventoryItem, vehicleId));
        }

        private static bool IsKeyForVehicle(InventoryItem inventoryItem, ulong vehicleId)
        {
            return inventoryItem.GetItem().IsItemVehicleKey()
                && inventoryItem.GetAdditionalData().GetUlong(VehicleIdKey) == vehicleId;
        }
    }
}
