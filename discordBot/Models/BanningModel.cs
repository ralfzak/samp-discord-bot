using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discordBot.Services;
using System.Linq;
using discordBot.Helpers;
using Discord.WebSocket;
using System.Threading;
using System.Net;

namespace discordBot.Models
{
    public class BanningModel : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Name("ban")]
        [Summary("/ban")]
        public async Task banAsync(IUser user = null, int days = 0, int hours = 0, int sendMessageToUser = 0, [RemainderAttribute]string reason = "No Reason Given")
        {
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username} /ban");

            if (Context.Guild == null)
            {
                await ReplyAsync("This only works on the server.");
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/ban [@]<user> [days] [hours] [send ban message] [reason] - issues a discord ban" +
                    "\n" +
                    "Set **days** & **hours** to 0 for a permanenet ban" +
                    "\n" +
                    "Set **send ban message** to 1 to direct message the user the ban reason and expire time, 0 not to: <https://i.imgur.com/2RUOa7L.png>");
                return;
            }

            var guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                await ReplyAsync("User not found.");
                return;
            }

            if (days < 0 || hours < 0)
            {
                await ReplyAsync("Invalid duration.");
                return;
            }

            if (sendMessageToUser != 0 && sendMessageToUser != 1)
            {
                await ReplyAsync("Set send user message to either 0 or 1.");
                return;
            }
            
            int daysToSeconds = (days * 86400);
            int hoursToSeconds = (hours * 3600);
            int timeToAdd = daysToSeconds + hoursToSeconds;
            int timedban = (days == 0 && hours == 0) ? 0 : 1;

            LoggerService.Write($"{Context.User.Username} /ban {guildUser.Id} {guildUser.Username} {daysToSeconds} {hoursToSeconds} {timeToAdd} {timedban}");

            // send messages
            var dmChannel = await guildUser.GetOrCreateDMChannelAsync();

            if (timedban == 1)
            {
                var expires_on = DateTime.Now.AddSeconds(timeToAdd).ToString("dddd, dd MMMM yyyy");

                await ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban will expire on {expires_on}.");

                if (sendMessageToUser == 1)
                    await dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for **{reason}**. This ban will expire on {expires_on}.");
            }
                
            else
            {
                await ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban is permanent.");

                if (sendMessageToUser == 1)
                    await dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for **{reason}**. This ban is permanent.");
            }

            await Context.Guild.AddBanAsync(guildUser, 0, "DO NOT LIFT THIS BAN HERE (use /banlookup on admin channel)");
            BanningService.StoreBan(guildUser.Id, guildUser.Username, Context.User.Id, Context.User.Username, timeToAdd, reason, timedban);
        }

        [Command("banlookup")]
        [Name("banlookup")]
        [Summary("/banlookup")]
        public async Task banlookupAsync(string user = "")
        {
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username} /banlookup {user}");

            if (Context.Guild == null)
            {
                await ReplyAsync("This only works on the server.");
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/banlookup <userid, username>" +
                    "\n" +
                    "Search by a ID -> Open the ban list, Right click, Copy ID");
                return;
            }

            var bans = BanningService.GetBans(user);
            if (bans.Count == 0)
            {
                await ReplyAsync("No bans found");
                return;
            }

            foreach (var ban in bans)
            {
                if (ban.expires_on != 0)
                {
                    var expires_on = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(ban.expires_on).ToString("dddd, dd MMMM yyyy");
                    await ReplyAsync($"<@{ban.uid}> ({ban.name}) by <@{ban.byuid}> ({ban.byname}) on **{ban.banned_on}** for **{ban.reason}**. Ban expires on **{expires_on}**.");
                }
                else
                    await ReplyAsync($"<@{ban.uid}> ({ban.name}) by <@{ban.byuid}> ({ban.byname}) on **{ban.banned_on}** for **{ban.reason}**. Ban is permanent.");
            }    
        }

        // Server 
        [Command("banlift")]
        [Name("banlift")]
        [Summary("/banlift")]
        public async Task banliftAsync(string user = "")
        {
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username} /banlift {user}");

            if (Context.Guild == null)
            {
                await ReplyAsync("This only works on the server.");
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/banlift <userid, username>" +
                    "\n" +
                    "Search by a ID -> Open the ban list, Right click, Copy ID");
                return;
            }

            var bans = BanningService.GetBans(user);
            if (bans.Count == 0)
            {
                await ReplyAsync("No bans found to lift.");
                return;
            }

            foreach (var ban in bans)
            {
                await Context.Guild.RemoveBanAsync(ban.uid);

                if (ban.expires_on != 0)
                {
                    var expires_on = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(ban.expires_on).ToString("dddd, dd MMMM yyyy");
                    await ReplyAsync($"<@{ban.uid}> ({ban.name}) by <@{ban.byuid}> ({ban.byname}) on **{ban.banned_on}** for **{ban.reason}**. Ban expires on **{expires_on}**. Lifted.");
                }
                else
                    await ReplyAsync($"<@{ban.uid}> ({ban.name}) by <@{ban.byuid}> ({ban.byname}) on **{ban.banned_on}** for **{ban.reason}**. Ban is permanent. Lifted.");

                BanningService.RemoveBan(ban.uid);

                LoggerService.Write($"{Context.User.Username} /banlift {user} - lifted {ban.uid}");
            }

        }
    }
}
