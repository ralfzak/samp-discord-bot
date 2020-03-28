using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace app.Services
{
    class EventHandlingService
    {
        private readonly DiscordSocketClient _discord;

        public EventHandlingService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            
            _discord.UserJoined += OnUserJoinServer;
            _discord.UserLeft += OnUserLeaveServer;

            LoggerService.Write("Binded the user (connect / disconnect) events!");
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task OnUserJoinServer(SocketGuildUser user)
        {
            if (UserService.IsUserVerified(user.Id))
            {
                var verifiedRole = _discord
                    .Guilds
                    .FirstOrDefault(g => g.Id == Program.GUILD_ID)
                    .Roles
                    .FirstOrDefault(r => r.Id == Program.VERIFIED_ROLE_ID);
                
                LoggerService.Write($"> JOIN VERIFIED: {user.Id} - ROLE SET");
                await user.AddRoleAsync(verifiedRole);
            }

            CacheService.ClearCache(user.Id);
            await Task.CompletedTask;
        }

        public async Task OnUserLeaveServer(SocketGuildUser user)
        {
            CacheService.ClearCache(user.Id);
            await Task.CompletedTask;
        }
    }
}
