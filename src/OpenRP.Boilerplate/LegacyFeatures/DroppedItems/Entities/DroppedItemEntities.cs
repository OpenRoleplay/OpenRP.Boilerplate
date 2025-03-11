using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Entities
{
    public static class DroppedItemEntities
    {
        [EntityType]
        public static readonly Guid DroppedItemType = new Guid("CDFF3D53-E1C7-4E1F-BD04-C5F41C34C32C");

        public static EntityId GetDroppedItemId(int droppedItemId)
        {
            return new EntityId(DroppedItemType, droppedItemId);
        }
    }
}
