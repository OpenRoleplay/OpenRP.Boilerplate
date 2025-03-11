using OpenRP.Framework.Database.Models;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Factions.Components
{
    public class Faction : Component
    {
        private FactionModel _cachedFactionModel;

        public Faction(FactionModel factionModel)
        {
            _cachedFactionModel = factionModel;
        }

        public ulong GetDatabaseId()
        {
            return _cachedFactionModel.Id;
        }

        public string GetFactionName()
        {
            return _cachedFactionModel.Name;
        }
    }
}
