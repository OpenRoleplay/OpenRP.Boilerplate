using OpenRP.Framework.Features.VirtualWorlds.Services;
using OpenRP.Framework.Database.Models;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Components
{
    public class Property : Component
    {
        private ulong _databaseId;
        private PropertyModel _cachedPropertyModel;
        private IVirtualWorldManager _virtualWorldManager;
        public Property(PropertyModel propertyModel, IVirtualWorldManager virtualWorldManager)
        {
            _cachedPropertyModel = propertyModel;
            _databaseId = _cachedPropertyModel.Id;
            _virtualWorldManager = virtualWorldManager;
        }

        public ulong GetDatabaseId()
        {
            return _databaseId;
        }

        public int GetPropertyVirtualWorld()
        {
            return _virtualWorldManager.GetPropertyVirtualWorld(_databaseId);
        }

        public string GetPropertyName()
        {
            return _cachedPropertyModel.Name;
        }

        public List<PropertyDoor> GetPropertyDoors()
        {
            return this.GetComponents<PropertyDoor>().ToList();
        }
    }
}
