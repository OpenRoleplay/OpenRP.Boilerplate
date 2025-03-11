using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Components;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Harvestables.Systems
{
    public class HarvestableSystem : ISystem
    {
        [Timer(1000)]
        public void HarvestableTimer(IEntityManager entityManager)
        {
            foreach (PlayerHarvesting playerHarvesting in entityManager.GetComponents<PlayerHarvesting>())
            {
                if(playerHarvesting.IsDone())
                {
                    playerHarvesting.Finish();
                }
            }
        }
    }
}
