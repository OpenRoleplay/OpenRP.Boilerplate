using SampSharp.Core;
using SampSharp.Entities;
using System.Diagnostics;
using System.Reflection;

namespace OpenRP.Boilerplate
{
    public class Program
    {
        static void Main(string[] args)
        {
            var appDirPath = GetApplicationLocation();

            ApplyAssemblyResolveFix(appDirPath);

            new GameModeBuilder()
                .UseEcs<Startup>()
                .Run();
        }

        private static string GetApplicationLocation()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Unable to determine the entry assembly.");
            var appDirPath = Path.GetDirectoryName(entryAssembly.Location);
            if (string.IsNullOrEmpty(appDirPath))
            {
                throw new InvalidOperationException("Unable to determine the application's location.");
            }
            return appDirPath;
        }

        private static void ApplyAssemblyResolveFix(string searchDirectory)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object? sender, ResolveEventArgs args) =>
            {
                Debug.WriteLine($"Resolving assembly: {args.Name}");

                var assemblyName = new AssemblyName(args.Name).Name;
                var resolvedPath = Path.Combine(searchDirectory, $"{assemblyName}.dll");

                if (File.Exists(resolvedPath))
                {
                    Debug.WriteLine($"Resolved path: {resolvedPath}");
                    return Assembly.LoadFile(resolvedPath);
                }

                Debug.WriteLine("Not resolved.");
                return null;
            };
        }
    }
}
