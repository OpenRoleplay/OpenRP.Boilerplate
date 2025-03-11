using Microsoft.Extensions.DependencyInjection;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Harvestables;

namespace OpenRP.Boilerplate.LegacyFeatures.AccountSettingsFeature.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorldGenerator(this IServiceCollection self)
        {
            return self
                .AddSingleton<IHarvestable, HempHarvest>();
        }
    }
}
