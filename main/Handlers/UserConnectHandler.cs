using main.Core;
using main.Services;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace main.Handlers
{
    /// <summary>
    /// Encapsulates user connect event for welcoming and verification role setting.
    /// </summary>
    #pragma warning disable 4014, 1998
    public class UserConnectHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly VerificationService _verificationService;
        private readonly CacheService _cacheService;
        private readonly ulong _guildId;
        private readonly ulong _verifiedRoleId;

        public UserConnectHandler(
            IServiceProvider services, 
            CacheService cacheService, 
            VerificationService verificationService)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _verificationService = verificationService;
            _cacheService = cacheService;
            _guildId = Configuration.GetVariable("Guild.Id");
            _verifiedRoleId = Configuration.GetVariable("Guild.VerifiedRoleId");

            _discord.UserJoined += OnUserJoinServer;
            _discord.UserLeft += OnUserLeaveServer;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task OnUserJoinServer(SocketGuildUser user)
        {
            var message = $"Hi {user.Mention}!  Welcome to **{user.Guild.Name}**.\n\n";

            if (_verificationService.IsUserVerified(user.Id))
            {
                var verifiedRole = _discord.GetGuild(_guildId).GetRole(_verifiedRoleId);
                message +=
                    "You have already verified your discord account and linked it to your forum profile! You have been set a Verified role, woohoo! :partying_face:";

                user.AddRoleAsync(verifiedRole);
                Logger.Write($"[OnUserJoinServer] {user.Id} has joined the server, verified role set");
            }
            else message += "You can link your forum account to your discord profile and get a Verified role.  Type `/verify` below to start the process!";

            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                dm.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Logger.Write($"[OnUserJoinServer] {user.Id} failed to send welcome msg: {e.Message}");
            }
            
            _cacheService.ClearCache(user.Id);
        }

        public async Task OnUserLeaveServer(SocketGuildUser user)
        {
            _cacheService.ClearCache(user.Id);
        }
    }
}
