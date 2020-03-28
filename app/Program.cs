using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using app.Services;

namespace app
{
    class Program
    {
        public static string FORUM_PROFILE_URL = "https://forum.sa-mp.com/member.php?u=";

        private static string BOT_TOKEN = 
            ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_BOT_TOKEN);
        
        public static ulong GUILD_ID = 
            ulong.Parse(ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_GUILD_ID));

        public static ulong VERIFIED_ROLE_ID = 
            ulong.Parse(ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_VERIFIED_ROLE_ID));

        public static ulong SCRIPTING_CHAN_ID = 
            ulong.Parse(ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_SCRIPTING_CHAN_ID));
        
        public static ulong ADVERT_CHAN_ID = 
            ulong.Parse(ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_ADVERT_CHAN_ID));
        
        public static ulong BOT_CHAN_ID = 
            ulong.Parse(ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_BOT_CHAN_ID));
        
        public static ulong ADMIN_CHAN_ID = 
            ulong.Parse(ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_ADMIN_CHAN_ID));

        public static string DB_SERVER =
            ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_DB_SERVER);

        public static string DB_DB =
            ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_DB_DB);

        public static string DB_USER =
            ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_DB_USER);

        public static string DB_PASS =
            ConfigurationService.GetConfigurationString(ConfigurationService.CONFIG_KEY_DB_PASS);

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