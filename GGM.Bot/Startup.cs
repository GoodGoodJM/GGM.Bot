using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using GGM.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GGM.Bot
{
    public class Startup
    {
        public static async Task RunAsync()
        {
            var startup = new Startup();
            await startup.Run();
        }

        private readonly IConfiguration _configuration;

        public Startup()
        {
            // Load Configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        private async Task Run()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            var applicationService = provider.GetRequiredService<Application>();
            await applicationService.Start();
            await applicationService.WhenTerminated();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration Register
            var botConfiguration = new BotConfiguration();
            _configuration.GetSection("Discord").Bind(botConfiguration);
            var commandServiceConfig = botConfiguration.ToCommandServiceConfig();

            services
                .AddSerilog(_configuration.GetSection("Serilog"))
                .AddSingleton(botConfiguration)
                .AddSingleton(new CommandService(commandServiceConfig))
                .AddSingleton<Application>();
        }
    }

    public static class ServiceExtension
    {
        public static IServiceCollection AddSerilog(this IServiceCollection self, IConfigurationSection configuration)
        {
            return self.AddLogging(loggingBuilder =>
            {
                // Log Setting
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.ConfigurationSection(configuration)
                    .CreateLogger();
                loggingBuilder.AddSerilog(dispose: true);
            });
        }
    }
}