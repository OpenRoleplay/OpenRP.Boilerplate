using OpenRP.Framework.Database.Models;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Entities;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.Streamer.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Components
{
    public class PropertyDoor : Component
    {
        private IStreamerService _streamerService;
        private IEntityManager _entityManager;
        private ulong _databaseId;
        private PropertyDoorModel _cachedPropertyDoorModel;
        private Property _property;
        public PropertyDoor(IStreamerService streamerService, IEntityManager entityManager, PropertyDoorModel propertyDoorModel)
        {
            _cachedPropertyDoorModel = propertyDoorModel;
            _databaseId = _cachedPropertyDoorModel.Id;
            _streamerService = streamerService;
            _entityManager = entityManager;
        }

        public Property GetProperty()
        {
            if (_property != null)
            {
                return _property;
            }

            if (this.Entity.IsOfType(PropertyEntities.PropertyType))
            {
                _property = _entityManager.GetComponent<Property>(this.Entity);
            }

            return _property;
        }

        public string GetPropertyDoorName()
        {
            return _cachedPropertyDoorModel?.Name;
        }

        public string GetPropertyDoorLabel()
        {
            string propertyName = GetProperty().GetPropertyName();
            string propertyDoorName = GetPropertyDoorName();

            if(!String.IsNullOrEmpty(propertyDoorName))
            {
                return $"{propertyDoorName} of {propertyName}";
            }

            return $"{propertyName}";
        }

        public int GetPropertyDoorInterior()
        {
            if(_cachedPropertyDoorModel.Interior != null && _cachedPropertyDoorModel.Interior.HasValue)
            {
                return _cachedPropertyDoorModel.Interior.Value;
            }

            return 0;
        }

        public Vector3 GetPropertyDoorPos()
        {
            return new Vector3(_cachedPropertyDoorModel.X, _cachedPropertyDoorModel.Y, _cachedPropertyDoorModel.Z);
        }

        public PropertyDoor GetPropertyDoorLinkedTo()
        {
            Property propertyLinkedTo = GetProperty();
            List<PropertyDoor> propertyDoors = propertyLinkedTo.GetPropertyDoors();

            return propertyDoors.FirstOrDefault(i => i._databaseId == _cachedPropertyDoorModel.LinkedToPropertyDoorId);
        }

        public void CreateOrUpdateTextLabel()
        {
            DynamicTextLabel label = this.GetComponent<DynamicTextLabel>();
            Property property = GetProperty();

            if (label != null)
            {
                // TODO: Update
            } 
            else
            {
                string labelText = GetPropertyDoorLabel();
                Vector3 labelPos = GetPropertyDoorPos();
                label = _streamerService.CreateDynamicTextLabel(labelText, Color.White, labelPos, 10.0f);
            }
        }

        public bool IsPlayerNearby(Player player)
        {
            if (player.IsInRangeOfPoint(3.0f, GetPropertyDoorPos()))
            {
                return true;
            }
            return false;
        }
    }
}
