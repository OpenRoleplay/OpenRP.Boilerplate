using OpenRP.Framework.Database.Models;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Components;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Services
{
    public interface IPropertyManager
    {
        void LoadAndUnloadProperties();
        Property LoadProperty(PropertyModel propertyModel);
        List<PropertyDoorModel> LoadPropertyDoors(PropertyModel propertyModel);
        IEnumerable<Property> GetAllProperties();
        IEnumerable<PropertyDoor> GetAllPropertyDoors();
    }
}
