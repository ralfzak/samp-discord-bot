using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using discordBot.Models;

namespace discordBot.Services
{
    class BanningService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private System.Threading.Timer BanTimer;

        public BanningService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            BanTimer = new System.Threading.Timer(this.OnBanCheckAsync, null, 60000, 600000);
            
            LoggerService.Write("The banning check has been hooked to BanTimer!");
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
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
                        await _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID).SendMessageAsync(
                            $"Lifted expired ban on <@{ban.uid}> ({ban.name}) that was issued on {ban.banned_on} by <@{ban.byuid}> ({ban.byname}) for {ban.reason}."
                            );

                        await _discord.GetGuild(Program.GUILD_ID).RemoveBanAsync(ban.uid);

                        // if ban really removed, remove from DB
                        RemoveBan(ban.uid);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerService.Write(e.ToString());

                await _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID).SendMessageAsync(
                    $"Exception in OnBanCheckAsync: {e.Message}"
                );
            }
        }

        public static void StoreBan(ulong uid, string name, ulong byuid, string byname, int secondsadd, string reason, int timedban)
        {
            DataService.Put($"INSERT INTO `bans` (`uid`, `name`, `byuid`, `byname`, `expires_on`, `expired`, `reason`) VALUES (@userid, @username, @byuserid, @byusername, ((UNIX_TIMESTAMP(NOW()) + @secondsadd) * @timedban), 'N', @reasonban)", new Dictionary<string, object>()
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
            DataService.Update($"UPDATE `bans` SET `expired` = 'Y' WHERE `uid`=@userid", new Dictionary<string, object>()
            {
                {"@userid", uid}
            });
        }

        public static List<BanModel> GetBans(string search)
        {
            List<BanModel> bans = new List<BanModel>();
            string isuid = "`name`=@search";
            if (Int64.TryParse(search, out long v))
            {
                isuid = "`uid`=@search";
            }

            var data = DataService.Get($"SELECT `uid`, `name`, `byuid`, `byname`, `expires_on`, DATE_FORMAT(banned_on, '%D %M %Y') `banned_on`, `reason` FROM `bans` WHERE {isuid} AND `expired` = 'N'", 
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
                        uid = (ulong)(long)data["uid"][i],
                        name = (string)data["name"][i],
                        byuid = (ulong)(long)data["byuid"][i],
                        byname = (string)data["byname"][i],
                        expires_on = (int)data["expires_on"][i],
                        banned_on = (string)data["banned_on"][i],
                        reason = (string)data["reason"][i],
                        expired = "N"
                    });
                }
            }
            return bans;
        }

        public static List<BanModel> GetExpiredBans()
        {
            List<BanModel> bans = new List<BanModel>();
            var data = DataService.Get($"SELECT `uid`, `name`, `byuid`, `byname`, `expires_on`, DATE_FORMAT(banned_on, '%D %M %Y') `banned_on`, `reason` FROM `bans` WHERE `expires_on` < UNIX_TIMESTAMP(NOW()) AND `expires_on` != 0 AND `expired` = 'N'", null);
            if (data.Count > 0)
            {
                for (int i = 0; i != data["uid"].Count; i++)
                {
                    bans.Add(new BanModel
                    {
                        uid = (ulong)(long)data["uid"][i],
                        name = (string)data["name"][i],
                        byuid = (ulong)(long)data["byuid"][i],
                        byname = (string)data["byname"][i],
                        expires_on = (int)data["expires_on"][i],
                        banned_on = (string)data["banned_on"][i],
                        reason = (string)data["reason"][i],
                        expired = "N"
                    });
                }
            }
            return bans;
        }
    }
}
