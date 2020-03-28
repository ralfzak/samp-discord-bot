using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using app.Services;
using app.Helpers;
using app.Models;
using Newtonsoft.Json;

namespace app
{
    class Program
    {
        private static bool PRODUCTION = true;
        private static string BOT_TOKEN = "NTY4MTMyMzM3NTczOTUzNTUy.XL41rQ.puaIj7V17qdwQfV35Hk2Mr05vu4";
        public static ulong BOT_ID = 568132337573953552;

        public static string FORUM_PROFILE_URL = "https://forum.sa-mp.com/member.php?u=";

        public static ulong GUILD_ID = 567064077613006861;

        public static ulong VERIFIED_ROLE_ID = 568478807791763483;

        public static ulong SCRIPTING_CHAN_ID = 567081165677264898;
        public static ulong ADVERT_CHAN_ID = 568123810897985538;
        public static ulong BOT_CHAN_ID = 570986182452838412;
        public static ulong ADMIN_CHAN_ID = 569907291965358091;
        
        public static string DB_SERVER = (PRODUCTION) ? "localhost" : "51.83.97.107";
        public static string DB_DB = "sampdiscord";
        public static string DB_USER = (PRODUCTION) ? "sampdiscord" : "sampdiscord";
        public static string DB_PASS = "sampdiscord";

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, BOT_TOKEN);
                await client.StartAsync();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                await services.GetRequiredService<EventHandlingService>().InitializeAsync();
                await services.GetRequiredService<BanningService>().InitializeAsync();
                await services.GetRequiredService<ServerAdPurgeService>().InitializeAsync();

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
                        MessageCacheSize = 50
                    });
                })
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>() // for handling cmds
                .AddSingleton<EventHandlingService>() // for handling events
                .AddSingleton<BanningService>() // for ban timer check
                .AddSingleton<ServerAdPurgeService>() // purge check
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}