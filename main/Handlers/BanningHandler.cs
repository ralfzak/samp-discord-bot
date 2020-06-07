using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using System.Linq;
using domain;
using main.Helpers;
using Discord;
using Discord.Rest;
using System.Threading;
using main.Services;

namespace main.Handlers
{
    #pragma warning disable 4014, 1998
    public class BanningHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly BanningService _banningService;
        private readonly ulong _guildId;
        private readonly ulong _adminChannelId;
        private Timer _banTimer;

        public BanningHandler(IServiceProvider services, Configuration configuration, BanningService banningService)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _banningService = banningService;
            _guildId = UInt64.Parse(configuration.GetVariable("GUILD_ID"));
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
            _banTimer = new Timer(OnBanCheckAsync, null, 60000, 600000);

            _discord.UserBanned += OnUserBanned;
            _discord.UserUnbanned += OnUserUnbanned;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        private async Task OnUserBanned(SocketUser user, SocketGuild server)
        {
            var banData = server.GetBanAsync(user.Id);
            var banningUser = GetBanningUserFromAuditLog(server, user.Id);
            var banReason = banData.Result.Reason ?? MessageHelper.NoReasonGiven;
            
            _banningService.StoreBan(
                banData.Result.User.Id, 
                banData.Result.User.Username, 
                banningUser.Id, 
                banningUser.Username, 
                0, 
                banReason
                );

            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                dmChannel.SendMessageAsync($"You have been banned from **{server.Name}**. This ban is permanent.");
            }
            catch (Exception e)
            {
                Logger.Write($"Failed to send ban notification to {user.Username}: {e.Message}");
            }
            
            Logger.Write($"[OnUserBanned] {banningUser.Username} banned {user.Username} for {banReason}");
        }

        private async Task OnUserUnbanned(SocketUser user, SocketGuild server)
        {
            var banData = _banningService.GetBans(user.Id.ToString());
            if (banData.Count > 0)
            {
                Logger.Write($"[OnUserUnbanned] {banData[0].Name} banned by {banData[0].ByName} for {banData[0].Reason}. Lifting...");
                _banningService.RemoveBan(user.Id);
            }
            
            Logger.Write($"[OnUserUnbanned] {user.Username}");
        }

        private async void OnBanCheckAsync(object state)
        {
            var expiredBans = _banningService.GetExpiredBans();
            if (expiredBans.Count > 0)
            {
                foreach (var ban in expiredBans)
                {
                    await _discord.GetGuild(_guildId).RemoveBanAsync(ban.Userid);

                    _discord.GetGuild(_guildId).GetTextChannel(_adminChannelId)
                        .SendMessageAsync($"Lifted expired ban on <@{ban.Userid}> ({ban.Name}) that was issued on **{ban.BannedOn.DateTime.ToHumanReadableString()}** by <@{ban.ByUserid}> ({ban.ByName}) for {ban.Reason}.");

                    _banningService.RemoveBan(ban.Userid);
                }
            }
            
            Logger.Write($"[OnBanCheckAsync] {expiredBans.Count} expired bans");
        }
        
        private IUser GetBanningUserFromAuditLog(SocketGuild server, ulong bannedId)
        {
            IUser banningUser = null;
            server.GetAuditLogsAsync(5).ForEach(logEntries =>
            {
                foreach (var logEntry in logEntries)
                {
                    if (logEntry.Action == ActionType.Ban)
                    {
                        if ((logEntry.Data as BanAuditLogData).Target.Id == bannedId)
                        {
                            banningUser = logEntry.User;
                            break;
                        }
                    }
                }
            });
            return banningUser;
        }
    }
}
