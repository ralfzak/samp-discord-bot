using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using domain.Models;
using main.Services;
using main.Modules;
using main.Core;
using main.Handlers;
using Microsoft.EntityFrameworkCore;

namespace main
{
    class Program
    {
        public static string FORUM_PROFILE_URL = "https://forum.sa-mp.com/member.php?u=";

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                var configuration = services.GetRequiredService<Configuration>();

                services.GetRequiredService<DatabaseContext>().Database.GetDbConnection().ConnectionString = 
                    BuildDbConnectionString(configuration);

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, configuration.GetVariable("BOT_TOKEN"));
                await client.StartAsync();

                await services.GetRequiredService<Commands>().InitializeAsync();

                await services.GetRequiredService<BanningHandler>().InitializeAsync();
                await services.GetRequiredService<UserConnectHandler>().InitializeAsync();
                await services.GetRequiredService<BotStatusHandler>().InitializeAsync();
                await services.GetRequiredService<MessageHandler>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private string BuildDbConnectionString(Configuration configuration)
        {
            return $"server={configuration.GetVariable("DB_SERVER")};" +
                   $"database={configuration.GetVariable("DB_DB")};" +
                   $"user={configuration.GetVariable("DB_USER")};" +
                   $"password={configuration.GetVariable("DB_PASS")};" +
                   $"port=3306;";
        }

        private Task LogAsync(LogMessage log)
        {
            Logger.Write(log.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>(_ =>
                {
                    return new DiscordSocketClient(new DiscordSocketConfig
                    {
                        MessageCacheSize = 150
                    });
                })
                .AddDbContext<DatabaseContext>()
                .AddSingleton<ITimeProvider, CurrentTimeProvider>()
                .AddSingleton<HttpClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<Configuration>()
                .AddSingleton<Commands>()

                .AddSingleton<BanningHandler>()
                .AddSingleton<BotStatusHandler>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<UserConnectHandler>()

                .AddSingleton<BanningModule>()
                .AddSingleton<HelpModule>()
                .AddSingleton<ServerInfoModule>()
                .AddSingleton<VerificationModule>()
                .AddSingleton<WikiModule>()

                .AddSingleton<BanningService>()
                .AddSingleton<CacheService>()
                .AddSingleton<MessageService>()
                .AddSingleton<UserService>()
                .AddSingleton<VerificationService>()
                .AddSingleton<WikiService>()

                .BuildServiceProvider();
        }
    }
}