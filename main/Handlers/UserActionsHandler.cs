using main.Core;
using main.Services;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;

namespace main.Handlers
{
    /// <summary>
    /// Encapsulates user connect event for welcoming and verification role setting.
    /// </summary>
    #pragma warning disable 4014, 1998
    public class UserActionsHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly VerificationService _verificationService;
        private readonly UserService _userService;
        private readonly CacheService _cacheService;
        private readonly ulong _guildId;

        public UserActionsHandler(
            IServiceProvider services, 
            CacheService cacheService, 
            UserService userService,
            VerificationService verificationService)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _verificationService = verificationService;
            _userService = userService;
            _cacheService = cacheService;
            _guildId = Configuration.GetVariable<ulong>(ConfigurationKeys.GuildId);

            _discord.UserJoined += OnUserJoinServer;
            _discord.UserLeft += OnUserLeaveServer;
            _discord.GuildMemberUpdated += OnUserUpdated;
        }

        private async Task OnUserUpdated(SocketUser oldUser, SocketUser newUser)
        {
            var guild = _discord.GetGuild(_guildId);
            var guildUser = guild.GetUser(newUser.Id);
            if (guildUser == null)
                return;
            
            var updatedRoles = guildUser.Roles.Where(r => !r.IsEveryone).Select(r => r.Id).ToList();
            var persistedRoles = _userService.GetUserRolesIds(guildUser.Id);
            var addedRoles = updatedRoles.Except(persistedRoles).ToList();
            var removedRoles = persistedRoles.Except(updatedRoles).ToList();
            
            addedRoles.ForEach(r =>
            {
                var assignedBy = GetRoleAssigningUserFromAuditLog(guild, guildUser.Id) ?? guildUser;
                _userService.AssignUserRole(guildUser.Id, r, assignedBy.Id);
            });
            
            removedRoles.ForEach(r =>
            {
                _userService.DeleteUserRole(guildUser.Id, r);
            });
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task OnUserJoinServer(SocketGuildUser user)
        {
            var message = $"Hi {user.Mention}!  Welcome to **{user.Guild.Name}**.\n\n";
            if (_verificationService.IsUserVerified(user.Id))
                message += "Wohoo! You have already verified your discord account and linked it to your forum profile! :partying_face:";
            else
                message += "You can link your forum account to your discord profile and get a Verified role.  Type `/verify` below to start the process!";

            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                dm.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Logger.Write($"[OnUserJoinServer] {user.Id} failed to send welcome msg: {e.Message}");
            }

            var userRoles = _userService.GetUserRolesIds(user.Id);
            userRoles.ForEach(role =>
            {
                var guildRole = _discord.GetGuild(_guildId).GetRole(role);
                try
                {
                    user.AddRoleAsync(guildRole);
                }
                catch (Exception e)
                {
                    Logger.Write($"[OnUserJoinServer] Failed to set role {role} for {user.Id}: {e.Message}");
                }
            });
            
            _cacheService.ClearCache(user.Id);
        }

        public async Task OnUserLeaveServer(SocketGuildUser user)
        {
            _cacheService.ClearCache(user.Id);
        }
        
        private IUser GetRoleAssigningUserFromAuditLog(SocketGuild server, ulong userId)
        {
            IUser assigningUser = null;
            server.GetAuditLogsAsync(10).ForEach(logEntries =>
            {
                if (logEntries == null)
                {
                    assigningUser = null;
                }
                else
                {
                    foreach (var logEntry in logEntries)
                    {
                        if (logEntry.Action == ActionType.MemberRoleUpdated)
                        {
                            if ((logEntry.Data as MemberRoleAuditLogData).Target.Id == userId)
                            {
                                assigningUser = logEntry.User;
                                break;
                            }
                        }
                    } 
                }
            });
            return assigningUser;
        }
    }
}
