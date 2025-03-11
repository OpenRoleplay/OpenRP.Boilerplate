using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenRP.Boilerplate.Configuration;
using OpenRP.Framework.Database;

namespace OpenRP.Boilerplate.Data
{
    public class DataContext : BaseDataContext
    {
        // Constructor
        public DataContext() { }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if options haven't been configured yet
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseMySql(
                        ConfigManager.Instance.Data.ConnectionString,
                        new MariaDbServerVersion(new Version(10, 4, 21)),
                        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
                    )
                    .LogTo(Console.WriteLine, LogLevel.Information);
            }
        }
    }
}
