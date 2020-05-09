using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using app.Services;
using app.Modules;
using app.Core;
using app.Handlers;

namespace app
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

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, configuration.GetVariable("BOT_TOKEN"));
                await client.StartAsync();

                await services.GetRequiredService<Commands>().InitializeAsync();

                await services.GetRequiredService<BanningHandler>().InitializeAsync();
                await services.GetRequiredService<VerifiedRoleHandler>().InitializeAsync();
                await services.GetRequiredService<BotStatusHandler>().InitializeAsync();
                await services.GetRequiredService<MessageHandler>().InitializeAsync();

                await services.GetRequiredService<DataService>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
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
                .AddSingleton<HttpClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<Configuration>()
                .AddSingleton<Commands>()

                .AddSingleton<BanningHandler>()
                .AddSingleton<BotStatusHandler>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<VerifiedRoleHandler>()

                .AddSingleton<BanningModule>()
                .AddSingleton<HelpModule>()
                .AddSingleton<ServerInfoModule>()
                .AddSingleton<VerificationModule>()
                .AddSingleton<WikiModule>()

                .AddSingleton<BanningService>()
                .AddSingleton<CacheService>()
                .AddSingleton<DataService>()
                .AddSingleton<MessageService>()
                .AddSingleton<UserService>()
                .AddSingleton<VerificationService>()
                .AddSingleton<WikiService>()

                .BuildServiceProvider();
        }
    }
}