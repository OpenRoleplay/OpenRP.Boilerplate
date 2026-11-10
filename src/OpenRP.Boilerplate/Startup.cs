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
using OpenRP.Framework.Features.ActorRoleplay.Extensions;
using OpenRP.Framework.Features.EmergencyCalls.Extensions;
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
using SampSharp.OpenMp.Core.Std;
using OpenRP.OpenMp.Streamer;
using System.Reflection;
using System.Text;

namespace OpenRP.Boilerplate
{
    public class Startup : IEcsStartup
    {
        public void Initialize(IStartupContext context)
        {
            // SA-MP clients show and send chat as Windows-1252, but SampSharp defaults to UTF-8, which garbles anything
            // outside plain ASCII (’ shows as â€™). Characters 1252 can't hold become "?".
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            StringViewMarshaller.Encoding = Encoding.GetEncoding(1252);

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
                // Written by ActorRoleplay's generator, so it uses the key and model set below.
                .AddActorConversations(options => { })
                .AddActorRoleplay(options =>
                {
                    options.OpenAiApiKey = ConfigManager.Instance.Data.OpenAI_API;
                    if (!String.IsNullOrWhiteSpace(ConfigManager.Instance.Data.OpenAI_Model))
                    {
                        options.ModelId = ConfigManager.Instance.Data.OpenAI_Model;
                    }
                    if (!String.IsNullOrWhiteSpace(ConfigManager.Instance.Data.OpenAI_ReasoningEffort))
                    {
                        options.ReasoningEffort = ConfigManager.Instance.Data.OpenAI_ReasoningEffort;
                    }
                })
                .AddEmergencyCalls(options =>
                {
                    options.OpenAiApiKey = ConfigManager.Instance.Data.OpenAI_API;
                    if (!String.IsNullOrWhiteSpace(ConfigManager.Instance.Data.OpenAI_Model))
                    {
                        options.ModelId = ConfigManager.Instance.Data.OpenAI_Model;
                    }
                    if (!String.IsNullOrWhiteSpace(ConfigManager.Instance.Data.OpenAI_ReasoningEffort))
                    {
                        options.ReasoningEffort = ConfigManager.Instance.Data.OpenAI_ReasoningEffort;
                    }
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
