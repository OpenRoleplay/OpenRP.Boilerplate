using SampSharp.Entities;
using SampSharp.Streamer.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Services
{
    public class ChickenCoopManager : IChickenCoopManager
    {
        private IEntityManager _entityManager;
        private IStreamerService _streamerService;

        public ChickenCoopManager(IEntityManager entityManager, IStreamerService streamerService)
        {
            _entityManager = entityManager;
            _streamerService = streamerService;
        }

        /// <summary>
        /// Creates and attaches a ChickenCoop component to the given dynamic object.
        /// </summary>
        /// <param name="dynamicObject">The dynamic object to which the ChickenCoop component will be attached.</param>
        /// <returns>The newly added ChickenCoop component.</returns>
        public ChickenCoop.Components.ChickenCoop CreateChickenCoop(DynamicObject dynamicObject)
        {
            // Use the AddComponent method on the dynamic object to create and attach a ChickenCoop
            return dynamicObject.AddComponent<ChickenCoop.Components.ChickenCoop>(_streamerService, dynamicObject);
        }

        /// <summary>
        /// Retrieves all ChickenCoop components currently managed by the entity manager.
        /// </summary>
        /// <returns>An enumerable of all ChickenCoop components.</returns>
        public IEnumerable<ChickenCoop.Components.ChickenCoop> GetAllChickenCoops()
        {
            return _entityManager.GetComponents<ChickenCoop.Components.ChickenCoop>();
        }
    }
}
