using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using app.Core;
using app.Helpers;
using Discord;
using Discord.Rest;
using System.Threading;
using app.Services;

namespace app.Handlers
{
    #pragma warning disable 4014, 1998
    public class BanningHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly Timer _banTimer;
        private readonly BanningService _banningService;
        private readonly ulong _guildId;
        private readonly ulong _adminChannelId;

        public BanningHandler(IServiceProvider services, Configuration configuration, BanningService banningService)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _banningService = banningService;
            _guildId = UInt64.Parse(configuration.GetVariable("GUILD_ID"));
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
            _banTimer = new Timer(this.OnBanCheckAsync, null, 60000, 600000);

            _discord.UserBanned += OnUserBanned;
            _discord.UserUnbanned += OnUserUnbanned;

            Logger.Write("The banning check has been hooked to BanTimer and ban monitoring events hooked!");
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        private async Task OnUserBanned(SocketUser user, SocketGuild server)
        {
            var banData = server.GetBanAsync(user.Id);
            var banningUser = GetBanningUserFromAuditLog(server.GetAuditLogsAsync(3), user.Id);
            var banReason = banData.Result.Reason ?? MessageHelper.NO_REASON_GIVEN;

            Logger.Write($"[OnUserBanned] {banningUser.Username} banned {user.Username} for {banReason}");

            // int daysToBan = 30 * 6;
            // int timeToAdd = (daysToBan * 86400);
            _banningService.StoreBan(banData.Result.User.Id, banData.Result.User.Username, banningUser.Id, banningUser.Username, 0, banReason, 0);

            // send user message
            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                // var expiresOn = DateTime.Now.AddSeconds(timeToAdd).ToString("dddd, dd MMMM yyyy");

                // await dmChannel.SendMessageAsync($"You have been banned from **{server.Name}**. This ban will expire on {expiresOn}.");
                dmChannel.SendMessageAsync($"You have been banned from **{server.Name}**. This ban is permanent.");
            }
            catch (Exception e)
            {
                Logger.Write($"Failed to send ban DM to {user.Username}: {e.Message}");
            }
        }

        private async Task OnUserUnbanned(SocketUser user, SocketGuild server)
        {
            Logger.Write($"[OnUserUnbanned] {user.Username}");

            var banData = _banningService.GetBans(user.Id.ToString());
            if (banData.Count > 0)
            {
                Logger.Write($"[OnUserUnbanned] {banData[0].Name} banned by {banData[0].ByName} for {banData[0].Reason}. Lifting...");
                _banningService.RemoveBan(user.Id);
            }
        }

        private IUser GetBanningUserFromAuditLog(IAsyncEnumerable<IReadOnlyCollection<RestAuditLogEntry>> auditLog, ulong bannedId)
        {
            IUser banningUser = null;
            auditLog.ForEach(logEntries =>
            {
                foreach (var logEntry in logEntries)
                {
                    if (logEntry.Action == ActionType.Ban)
                    {
                        var banAuditLogData = logEntry.Data as BanAuditLogData;
                        if (banAuditLogData.Target.Id == bannedId)
                        {
                            banningUser = logEntry.User;
                            break;
                        }
                    }
                }
            });
            return banningUser;
        }

        private async void OnBanCheckAsync(object state)
        {
            try
            {
                var expiredBans = _banningService.GetExpiredBans();
                Logger.Write($"[OnBanCheck] invoked: {expiredBans.Count} expired bans");

                if (expiredBans.Count > 0)
                {
                    foreach (var ban in expiredBans)
                    {
                        await _discord.GetGuild(_guildId).RemoveBanAsync(ban.UId);

                        _discord.GetGuild(_guildId).GetTextChannel(_adminChannelId)
                            .SendMessageAsync($"Lifted expired ban on <@{ban.UId}> ({ban.Name}) that was issued on {ban.BannedOn} by <@{ban.ByUId}> ({ban.ByName}) for {ban.Reason}.");

                        _banningService.RemoveBan(ban.UId);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write($"Exception in OnBanCheckAsync: {e.Message}");
            }
        }
    }
}
