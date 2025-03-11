using OpenRP.Boilerplate.Data;
using SampSharp.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Factions.Components;
using OpenRP.Boilerplate.LegacyFeatures.Factions.Entities;
using OpenRP.Framework.Database.Models;

namespace OpenRP.Boilerplate.LegacyFeatures.Factions.Services
{
    public class FactionManager : IFactionManager
    {
        private IEntityManager _entityManager;
        public FactionManager(IEntityManager entityManager) 
        { 
            _entityManager = entityManager;
        }

        public void LoadAndUnloadFactions()
        {
            List<ulong> currentlyLoadedFactionIds = GetCurrentlyLoadedFactionDatabaseIds();

            // Fetch all vehicles except those with the specified IDs
            using (DataContext context = new DataContext())
            {
                List<FactionModel> factionModels = context.Factions
                    .Where(faction => !currentlyLoadedFactionIds.Contains(faction.Id))
                    .ToList();

                foreach (FactionModel faction in factionModels)
                {
                    Faction loadedFaction = LoadFaction(faction);
                }
            }
        }

        public Faction LoadFaction(FactionModel factionModel)
        {
            // Add Component
            EntityId factionEntityId = FactionEntities.GetFactionId((int)factionModel.Id);
            _entityManager.Create(factionEntityId);

            Faction property = _entityManager.AddComponent<Faction>(factionEntityId, factionModel);

            return property;
        }

        private List<ulong> GetCurrentlyLoadedFactionDatabaseIds()
        {
            List<ulong> loadedPropertyDatabaseIds = new List<ulong>();
            foreach (Faction loadedProperty in _entityManager.GetComponents<Faction>())
            {
                loadedPropertyDatabaseIds.Add(loadedProperty.GetDatabaseId());
            }
            return loadedPropertyDatabaseIds;
        }
    }
}
