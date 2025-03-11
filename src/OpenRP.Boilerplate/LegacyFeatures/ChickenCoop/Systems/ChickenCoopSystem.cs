using OpenRP.Framework.Features.CDN.Services;
using OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Services;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Systems
{
    public class ChickenCoopSystem : ISystem
    {
        [Timer(60000 * 15)]
        public void UpdateChickenCoops(IChickenCoopManager chickenCoopManager, IEntityManager entityManager, IOpenCdnService openCdnService)
        {
            foreach (ChickenCoop.Components.ChickenCoop chickenCoop in chickenCoopManager.GetAllChickenCoops())
            {
                chickenCoop.UpdateChickenCoop(entityManager, openCdnService);
            }
        }
    }
}
