using OpenRP.Boilerplate.LegacyFeatures.Factions.Services;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Factions.Systems
{
    public class FactionSystem : ISystem
    {
        [Event]
        public void OnGameModeInit(IFactionManager factionManager)
        {
            factionManager.LoadAndUnloadFactions();
        }
    }
}
