using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRP.Boilerplate.Configuration;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Services;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Services;
using OpenRP.Boilerplate.LegacyFeatures.DroppedItems.Services;
using OpenRP.Boilerplate.LegacyFeatures.Factions.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Services;
using OpenRP.Boilerplate.LegacyFeatures.Vehicles.Managers;
using OpenRP.Framework.Database;
using OpenRP.Framework.Extensions;
using OpenRP.Framework.Features.CDN.Extensions;
using OpenRP.Framework.Features.Discord.Extensions;
using OpenRP.Framework.Features.Vehicles.Services;
using OpenRP.Framework.Features.WorldTime.Extensions;
using OpenRP.Framework.Features.WorldWeather.Extensions;
using OpenRP.Framework.Shared.ServerEvents.Extensions;
using SampSharp.ColAndreas.Entities.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.Streamer.Entities;
using SampSharp.Tryg3D.Entities.Services;

namespace OpenRP.Boilerplate
{
    public class Startup : IStartup
    {
        public void Configure(IServiceCollection services) =>
            // Register Database Context
            services
                .AddDbContext<DataContext>(options =>
                    options.UseMySql(
                        ConfigManager.Instance.Data.ConnectionString,
                        new MariaDbServerVersion(new Version(10, 4, 21)),
                        mysqlOptions => mysqlOptions
                            .EnableRetryOnFailure()
                    )
                    .LogTo(Console.WriteLine, LogLevel.Information),
                    ServiceLifetime.Transient
                )
                .AddTransient<BaseDataContext>(provider => provider.GetService<DataContext>())
                .AddMemoryCache()
                .AddEFSecondLevelCache(options =>
                    options.UseMemoryCacheProvider()
                        .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(15)))
                .AddSingleton<IStreamerService, StreamerService>()
                .AddSingleton<IColAndreasService, ColAndreasService>()
                .AddSingleton<ITryg3DService, Tryg3DService>()
                .AddTransient<ICharacterService, CharacterService>()
                .AddTransient<ICharacterVehicleManager, CharacterVehicleManager>()
                .AddTransient<ICurrencyService, CurrencyService>()
                .AddTransient<IInventoryService, InventoryService>()
                .AddSingleton<IDroppedItemService, DroppedItemService>()
                .AddSingleton<IFactionManager, FactionManager>()
                .AddSingleton<IPropertyManager, PropertyManager>()
                .AddSingleton<IChickenCoopManager, ChickenCoopManager>()
                .AddWorldWeather(options => { })
                .AddWorldTime(options => { })
                .AddCDN(options => { })
                .AddDiscord(options => { })
                .AddOpenRoleplayFramework()
                .AddSystemsInAssembly()
                .AddServerSystemsInAssembly();

        public void Configure(IEcsBuilder builder)
        {
            // TODO: Enable desired ECS system features
            builder.EnableSampEvents()
                .EnablePlayerCommands()
                .EnableRconCommands()
                .EnableStreamerEvents();
        }
    }
}
