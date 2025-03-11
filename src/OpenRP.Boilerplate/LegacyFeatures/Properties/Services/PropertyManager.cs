using OpenRP.Boilerplate.Data;
using SampSharp.Entities;
using SampSharp.Streamer.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Components;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Services;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.VirtualWorlds.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Services
{
    public class PropertyManager : IPropertyManager
    {
        private IEntityManager _entityManager;
        private IStreamerService _streamerService;
        private IVirtualWorldManager _virtualWorldManager;

        public PropertyManager(IEntityManager entityManager, IStreamerService streamerService, IVirtualWorldManager virtualWorldManager)
        {
            _entityManager = entityManager;
            _streamerService = streamerService;
            _virtualWorldManager = virtualWorldManager;
        }

        public void LoadAndUnloadProperties()
        {
            List<ulong> currentlyLoadedPropertyIds = GetCurrentlyLoadedPropertyDatabaseIds();

            // Fetch all vehicles except those with the specified IDs
            using (DataContext context = new DataContext())
            {
                List<PropertyModel> properties = context.Properties
                    .Where(property => !currentlyLoadedPropertyIds.Contains(property.Id))
                    .ToList();

                foreach (PropertyModel property in properties)
                {
                    Property loadedProperty = LoadProperty(property);
                }
            }
        }

        public Property LoadProperty(PropertyModel propertyModel)
        {
            // Add Component
            EntityId propertyEntityId = PropertyEntities.GetPropertyId((int)propertyModel.Id);
            _entityManager.Create(propertyEntityId);

            Property property = _entityManager.AddComponent<Property>(propertyEntityId, propertyModel, _virtualWorldManager);

            // Place Doors
            List<PropertyDoorModel> propertyDoorModels = LoadPropertyDoors(propertyModel);
            foreach (PropertyDoorModel propertyDoorModel in propertyDoorModels)
            {
                // Add Component
                PropertyDoor propertyDoor = property.AddComponent<PropertyDoor>(_streamerService, _entityManager, propertyDoorModel);
                propertyDoor.CreateOrUpdateTextLabel();
            }

            return property;
        }

        public List<PropertyDoorModel> LoadPropertyDoors(PropertyModel propertyModel)
        {
            List<PropertyDoorModel> propertyDoorModels = new List<PropertyDoorModel>();
            using (DataContext context = new DataContext())
            {
                propertyDoorModels = context.PropertyDoors.Where(i => i.PropertyId == propertyModel.Id).ToList();
            }
            return propertyDoorModels;
        }

        private List<ulong> GetCurrentlyLoadedPropertyDatabaseIds()
        {
            List<ulong> loadedPropertyDatabaseIds = new List<ulong>();
            foreach (Property loadedProperty in _entityManager.GetComponents<Property>())
            {
                loadedPropertyDatabaseIds.Add(loadedProperty.GetDatabaseId());
            }
            return loadedPropertyDatabaseIds;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return _entityManager.GetComponents<Property>();
        }

        public IEnumerable<PropertyDoor> GetAllPropertyDoors()
        {
            return _entityManager.GetComponents<PropertyDoor>();
        }
    }
}
