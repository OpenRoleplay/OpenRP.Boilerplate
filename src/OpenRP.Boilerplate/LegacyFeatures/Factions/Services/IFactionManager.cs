using OpenRP.Framework.Database.Models;
using OpenRP.Boilerplate.LegacyFeatures.Factions.Components;

namespace OpenRP.Boilerplate.LegacyFeatures.Factions.Services
{
    public interface IFactionManager
    {
        public void LoadAndUnloadFactions();
        public Faction LoadFaction(FactionModel factionModel);
    }
}
