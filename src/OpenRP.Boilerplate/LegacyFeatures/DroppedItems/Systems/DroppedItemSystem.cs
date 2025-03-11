using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Systems
{
    public class DroppedItemSystem : ISystem
    {
        [Event]
        public void OnGameModeInit(IDroppedItemService droppedItemService)
        {
            droppedItemService.LoadDroppedItems();
        }
    }
}
