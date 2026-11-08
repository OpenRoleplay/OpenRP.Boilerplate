using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRP.Boilerplate.Configuration;
using OpenRP.Boilerplate.Data;
using OpenRP.Framework.Database;
using OpenRP.Framework.Extensions;
using OpenRP.Framework.Features.ActorConversations.Extensions;
using OpenRP.Framework.Features.CDN.Extensions;
using OpenRP.Framework.Features.Discord.Extensions;
using OpenRP.Framework.Features.Vehicles.Services;
using OpenRP.Framework.Features.WorldTime.Extensions;
using OpenRP.Framework.Features.WorldWeather.Extensions;
using OpenRP.Framework.Shared.ServerEvents.Extensions;
using SampSharp.ColAndreas.Entities.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.Entities.SAMP.Commands;
using SampSharp.OpenMp.Core;
using OpenRP.OpenMp.Streamer;
using System.Reflection;

namespace OpenRP.Boilerplate
{
    public class Startup : IEcsStartup
    {
        public void Initialize(IStartupContext context)
        {
            context.UseEntities().UseCommands();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration) =>
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
                .AddStreamer()
                .AddColAndreas()
                .AddOpenRoleplayFramework()
                .AddActorConversations(options =>
                {
                    options.ChatGptApiKey = ConfigManager.Instance.Data.OpenAI_API;
                })
                .AddWorldWeather(options => { })
                .AddWorldTime(options => { })
                .AddCDN(options => { })
                .AddDiscord(options => { })
                // This assembly's ISystems are loaded by SampSharp itself, because it's the entry assembly.
                .AddServerSystemsInAssembly(typeof(Startup).Assembly);

        public void Configure(IEcsBuilder builder)
        {
            // Player and console commands are enabled by UseCommands() in Initialize.
            builder.EnableStreamerEvents();
        }
    }
}
