using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;

namespace JRMP.RVTool.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<App>().Run(args);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = LoadConfiguration();
            services.AddSingleton(configuration);
            services.AddTransient<App>();

            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("JRMP.RVTool.CLI.json", optional: true, reloadOnChange: true);

            Debug.WriteLine($"Loaded Configuration: \r\n{Directory.GetCurrentDirectory()}\\JRMP.RVTool.CLI.json");

            return builder.Build();
        }
    }
}

