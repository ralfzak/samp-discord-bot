using app.Core;
using app.Services;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace app.Handlers
{
    #pragma warning disable 4014, 1998
    public class UserConnectHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly UserService _userService;
        private readonly CacheService _cacheService;
        private readonly ulong _guildId;
        private readonly ulong _verifiedRoleId;

        public UserConnectHandler(IServiceProvider services, Configuration configuration, CacheService cacheService, UserService userService)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _userService = userService;
            _cacheService = cacheService;
            _guildId = UInt64.Parse(configuration.GetVariable("GUILD_ID"));
            _verifiedRoleId = UInt64.Parse(configuration.GetVariable("VERIFIED_ROLE_ID"));

            _discord.UserJoined += OnUserJoinServer;
            _discord.UserLeft += OnUserLeaveServer;

            Logger.Write("Binded the user (connect / disconnect) verification events!");
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task OnUserJoinServer(SocketGuildUser user)
        {
            var message = $"Hi {user.Mention}!  Welcome to **{user.Guild.Name}**.\n\n";

            if (_userService.IsUserVerified(user.Id))
            {
                var verifiedRole = _discord.GetGuild(_guildId).GetRole(_verifiedRoleId);
                message +=
                    "You have already verified your discord account and linked it to your forum profile! You have been set a Verified role, woohoo! :partying_face:";

                Logger.Write($"{user.Id} has joined the server, verified role set");
                user.AddRoleAsync(verifiedRole);
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
