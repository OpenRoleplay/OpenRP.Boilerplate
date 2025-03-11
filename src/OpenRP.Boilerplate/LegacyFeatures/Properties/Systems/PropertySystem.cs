using OpenRP.Boilerplate.LegacyFeatures.Properties.Services;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Systems
{
    public class PropertySystem : ISystem
    {
        [Event]
        public void OnGameModeInit(IPropertyManager propertyManager)
        {
            propertyManager.LoadAndUnloadProperties();
        }
    }
}
