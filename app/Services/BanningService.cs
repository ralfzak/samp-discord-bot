using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using app.Helpers;
using app.Models;
using Discord;
using Discord.Rest;

namespace app.Services
{
    class BanningService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private System.Threading.Timer _banTimer;

        public BanningService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            
            _banTimer = new System.Threading.Timer(this.OnBanCheckAsync, null, 60000, 600000);
            _discord.UserBanned += OnUserBanned;
            _discord.UserUnbanned += OnUserUnbanned;
            
            LoggerService.Write("The banning check has been hooked to BanTimer and ban monitoring events hooked!");
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }
        
        private async Task OnUserBanned(SocketUser user, SocketGuild server)
        {
            var banData = server.GetBanAsync(user.Id);
            var banningUser = GetBanningUserFromAuditLog(server.GetAuditLogsAsync(5), user.Id);
            var banReason = banData.Result.Reason ?? MessageHelper.NO_REASON_GIVEN;
            
            LoggerService.Write($"[OnUserBanned] {banningUser.Username} banned {user.Username} for {banReason}");
            
            // store ban in DB
            // int daysToBan = 30 * 6;
            // int timeToAdd = (daysToBan * 86400);
            StoreBan(
                banData.Result.User.Id, 
                banData.Result.User.Username,
                banningUser.Id,
                banningUser.Username,
                0,
                banReason,
                0);

            // send user message
            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                // var expiresOn = DateTime.Now.AddSeconds(timeToAdd).ToString("dddd, dd MMMM yyyy");
                    
                // await dmChannel.SendMessageAsync($"You have been banned from **{server.Name}**. This ban will expire on {expiresOn}.");
                await dmChannel.SendMessageAsync($"You have been banned from **{server.Name}**. This ban is permanent.");
            }
            catch (Exception e)
            {
                LoggerService.Write($"Failed to send ban DM to {user.Username} : {e.Message}");
            }

            await Task.CompletedTask;
        }

        private async Task OnUserUnbanned(SocketUser user, SocketGuild server)
        {
            LoggerService.Write($"[OnUserUnbanned] {user.Username}");
            
            var banData = GetBans(user.Id.ToString());
            if (banData.Count > 0)
            {
                LoggerService.Write($"[OnUserUnbanned] {banData[0].Name} banned by {banData[0].ByName} for {banData[0].Reason}. Lifting...");
                RemoveBan(user.Id);
            }

            await Task.CompletedTask;
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
                        LoggerService.Write($"LogEntry: {banAuditLogData.Target.Username} by {logEntry.User.Username} " +
                                            $"for {logEntry.Action.ToString()}");
                        
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
                var expiredBans = GetExpiredBans();
                LoggerService.Write($"[OnBanCheck] invoked: {expiredBans.Count} expired bans");

                if (expiredBans.Count > 0)
                {
                    foreach (var ban in expiredBans)
                    {
                        // send chan message & lift
                        await _discord
                            .GetGuild(Program.GUILD_ID)
                            .GetTextChannel(Program.ADMIN_CHAN_ID)
                            .SendMessageAsync($"Lifted expired ban on <@{ban.UId}> ({ban.Name}) that was issued on " +
                                              $"{ban.BannedOn} by <@{ban.ByUId}> ({ban.ByName}) for {ban.Reason}.");

                        await _discord.GetGuild(Program.GUILD_ID).RemoveBanAsync(ban.UId);

                        // if ban really removed, remove from DB
                        RemoveBan(ban.UId);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerService.Write(e.ToString());

                await _discord
                    .GetGuild(Program.GUILD_ID)
                    .GetTextChannel(Program.ADMIN_CHAN_ID)
                    .SendMessageAsync($"Exception in OnBanCheckAsync: {e.Message}");
            }
        }

        public static void StoreBan(ulong uid, string name, ulong byuid, string byname, int secondsadd, string reason, int timedban)
        {
            DataService.Put(
                "INSERT INTO `bans` (`uid`, `name`, `byuid`, `byname`, `expires_on`, `expired`, `reason`) VALUES (@userid, @username, @byuserid, @byusername, ((UNIX_TIMESTAMP(NOW()) + @secondsadd) * @timedban), 'N', @reasonban)", 
                new Dictionary<string, object>()
                {
                    { "@userid", uid },
                    { "@username", name },
                    { "@byuserid", byuid },
                    { "@byusername", byname },
                    { "@secondsadd", secondsadd },
                    { "@reasonban", reason },

                    { "@timedban", timedban }
                });
        }

        public static void RemoveBan(ulong uid)
        {
            DataService.Update("UPDATE `bans` SET `expired` = 'Y' WHERE `uid`=@userid", new Dictionary<string, object>()
            {
                {"@userid", uid}
            });
        }

        public static List<BanModel> GetBans(string search)
        {
            List<BanModel> bans = new List<BanModel>();
            string nameUidSearch = "`name`=@search";
            if (Int64.TryParse(search, out long v))
            {
                nameUidSearch = "`uid`=@search";
            }

            var data = DataService.Get(
                $"SELECT `uid`, `name`, `byuid`, `byname`, `expires_on`, DATE_FORMAT(banned_on, '%D %M %Y') `banned_on`, `reason` FROM `bans` WHERE {nameUidSearch} AND `expired` = 'N'", 
                new Dictionary<string, object>()
                {
                    {"@search", search}
                });

            if (data.Count > 0)
            {
                for (int i = 0; i != data["uid"].Count; i++)
                {
                    bans.Add(new BanModel
                    {
                        UId = (ulong)(long)data["uid"][i],
                        Name = (string)data["name"][i],
                        ByUId = (ulong)(long)data["byuid"][i],
                        ByName = (string)data["byname"][i],
                        ExpiresOn = (long)data["expires_on"][i],
                        BannedOn = (string)data["banned_on"][i],
                        Reason = (string)data["reason"][i],
                        Expired = "N"
                    });
                }
            }
            return bans;
        }

        public static List<BanModel> GetExpiredBans()
        {
            List<BanModel> bans = new List<BanModel>();
            var data = DataService.Get("SELECT `uid`, `name`, `byuid`, `byname`, `expires_on`, DATE_FORMAT(banned_on, '%D %M %Y') `banned_on`, `reason` FROM `bans` WHERE `expires_on` < UNIX_TIMESTAMP(NOW()) AND `expires_on` != 0 AND `expired` = 'N'", null);
            if (data.Count > 0)
            {
                for (int i = 0; i != data["uid"].Count; i++)
                {
                    bans.Add(new BanModel
                    {
                        UId = (ulong)(long)data["uid"][i],
                        Name = (string)data["name"][i],
                        ByUId = (ulong)(long)data["byuid"][i],
                        ByName = (string)data["byname"][i],
                        ExpiresOn = (int)data["expires_on"][i],
                        BannedOn = (string)data["banned_on"][i],
                        Reason = (string)data["reason"][i],
                        Expired = "N"
                    });
                }
            }
            return bans;
        }
    }
}
