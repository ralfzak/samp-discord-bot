using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using main.Services;
using main.Modules;
using main.Core;
using main.Handlers;
using Microsoft.EntityFrameworkCore;
using main.Core.Database;
using main.Core.Domain.Repo;

namespace main
{
    /// <summary>
    /// Main application class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main application entry point
        /// </summary>
        static void Main(string[] args)  
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                EnsureMigrations(services.GetRequiredService<DatabaseContext>());
                
                var client = services.GetRequiredService<DiscordSocketClient>();
                client.Log += LogAsync;
                string token = Configuration.GetVariable(ConfigurationKeys.BotToken);
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                services.GetRequiredService<CommandService>().Log += LogAsync;
                
                await services.GetRequiredService<Commands>().InitializeAsync();
                await services.GetRequiredService<BanningHandler>().InitializeAsync();
                await services.GetRequiredService<UserActionsHandler>().InitializeAsync();
                await services.GetRequiredService<BotStatusHandler>().InitializeAsync();
                await services.GetRequiredService<MessageHandler>().InitializeAsync();
                
                await Task.Delay(-1);
            }
        }
        
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 150
                }))
                .AddDbContext<DatabaseContext>(options =>
                {
                    options.UseMySQL(DbConnectionString());
                }, ServiceLifetime.Transient)
                .AddSingleton<IBansRepository, MysqlBansRepository>()
                .AddSingleton<IVerificationsRepository, MysqlVerificationsRepository>()
                .AddSingleton<IUserRoleRepository, MysqlUserRoleRepository>()
                
                .AddSingleton<ITimeProvider, CurrentTimeProvider>()
                .AddSingleton<IHttpClient, HttpClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<Commands>()

                .AddSingleton<BanningHandler>()
                .AddSingleton<BotStatusHandler>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<UserActionsHandler>()

                .AddSingleton<BanningModule>()
                .AddSingleton<HelpModule>()
                .AddSingleton<ServerInfoModule>()
                .AddSingleton<VerificationModule>()
                .AddSingleton<WikiModule>()

                .AddSingleton<BanningService>()
                .AddSingleton<CacheService>()
                .AddSingleton<MessageService>()
                .AddSingleton<SampServerService>()
                .AddSingleton<StatsService>()
                .AddSingleton<UserService>()
                .AddSingleton<VerificationService>()
                .AddSingleton<WikiService>()

                .BuildServiceProvider();
        }
        
        private void EnsureMigrations(DatabaseContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (`MigrationId` VARCHAR(150) NOT NULL, `ProductVersion` VARCHAR(32) NOT NULL, PRIMARY KEY (`MigrationId`));");
            dbContext.Database.Migrate();
        }
        
        private string DbConnectionString()
        {
            return $"server={Configuration.GetVariable(ConfigurationKeys.DatabaseHost)};" +
                   $"database={Configuration.GetVariable(ConfigurationKeys.DatabaseSchema)};" +
                   $"user={Configuration.GetVariable(ConfigurationKeys.DatabaseUser)};" +
                   $"password={Configuration.GetVariable(ConfigurationKeys.DatabasePassword)};" +
                   "port=3306;";
        }

        private Task LogAsync(LogMessage log)
        {
            Logger.Write(log.ToString());
            return Task.CompletedTask;
        }
    }
}
