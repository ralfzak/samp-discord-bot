using System;
using System.Threading.Tasks;
using app.Helpers;
using Discord;
using Discord.Commands;
using app.Services;

namespace app.Modules
{
    public class BanningModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Name("ban")]
        [Summary("/ban")]
        public async Task Ban(IUser user = null, 
            int days = 0, 
            int hours = 0,  
            int sendMessageToUser = 0,  
            [RemainderAttribute]string reason = MessageHelper.NO_REASON_GIVEN)
        {
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username}: /ban");
            if (Context.Guild == null)
            {
                await ReplyAsync(MessageHelper.COMMAND_SERVER_ONLY);
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/ban [@]<user> [days] [hours] [send ban message] [reason] - issues a discord ban" + 
                                 "\n" + 
                                 "Set **days** & **hours** to 0 for a permanent ban" + 
                                 "\n" + 
                                 "Set **send ban message** to 1 to direct message the user the ban reason and " +
                                 "expire time, 0 not to: <https://i.imgur.com/2RUOa7L.png>");
                return;
            }

            var guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                await ReplyAsync(MessageHelper.USER_NOT_FOUND);
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
            int isTimedBan = (days == 0 && hours == 0) ? 0 : 1;
            var dmChannel = await guildUser.GetOrCreateDMChannelAsync();
            
            if (isTimedBan == 1)
            {
                var expiresOn = DateTime.Now.AddSeconds(timeToAdd).ToString("dddd, dd MMMM yyyy");
                await ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. " +
                                 $"Ban will expire on {expiresOn}.");
                
                if (sendMessageToUser == 1)
                    await dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for " +
                                                     $"**{reason}**. This ban will expire on {expiresOn}.");
            }
            else
            {
                await ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban is permanent.");
                if (sendMessageToUser == 1)
                    await dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for " +
                                                     $"**{reason}**. This ban is permanent.");
            }

            await Context.Guild.AddBanAsync(guildUser, 
                0, 
                $"DO NOT LIFT THIS BAN HERE (use /banlookup on <#{Program.BOT_CHAN_ID}> channel)");
            
            BanningService.StoreBan(guildUser.Id, 
                guildUser.Username, 
                Context.User.Id, 
                Context.User.Username, 
                timeToAdd, 
                reason, 
                isTimedBan);
            
            LoggerService.Write($"{Context.User.Username}: /ban {guildUser.Id} {guildUser.Username} {daysToSeconds} " +
                                $"{hoursToSeconds} {timeToAdd} {isTimedBan}");
        }

        [Command("banlookup")]
        [Name("banlookup")]
        [Summary("/banlookup")]
        public async Task Banlookup(string user = "")
        {
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username}: /banlookup {user}");
            if (Context.Guild == null)
            {
                await ReplyAsync(MessageHelper.COMMAND_SERVER_ONLY);
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/banlookup <userid, username>" +
                    "\n" +
                    "Search by a ID (*Open the ban list, Right click, Copy ID*)");
                return;
            }

            var bans = BanningService.GetBans(user);
            if (bans.Count == 0)
            {
                await ReplyAsync("No bans found.");
                return;
            }

            bans.ForEach(async ban =>
            {
                if (ban.ExpiresOn != 0)
                {
                    var expiresOnDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)
                        .AddSeconds(ban.ExpiresOn)
                        .ToString("dddd, dd MMMM yyyy");

                    await ReplyAsync($"<@{ban.UId}> ({ban.Name}) by <@{ban.ByUId}> ({ban.ByName}) on " +
                                     $"**{ban.BannedOn}** for **{ban.Reason}**. Ban expires on **{expiresOnDate}**.");
                }
                else
                    await ReplyAsync($"<@{ban.UId}> ({ban.Name}) by <@{ban.ByUId}> ({ban.ByName}) on " +
                                     $"**{ban.BannedOn}** for **{ban.Reason}**. Ban is permanent.");
            });
        }

        // Server 
        [Command("banlift")]
        [Name("banlift")]
        [Summary("/banlift")]
        public async Task Banlift(string user = "")
        {
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username}: /banlift {user}");
            if (Context.Guild == null)
            {
                await ReplyAsync(MessageHelper.COMMAND_SERVER_ONLY);
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/banlift <userid, username>" + 
                                 "\n" + 
                                 "Search by a ID (*Open the ban list, Right click, Copy ID*)");
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
                await Context.Guild.RemoveBanAsync(ban.UId);

                if (ban.ExpiresOn != 0)
                {
                    var expiryDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)
                        .AddSeconds(ban.ExpiresOn)
                        .ToString("dddd, dd MMMM yyyy");
                    
                    await ReplyAsync($"<@{ban.UId}> ({ban.Name}) by <@{ban.ByUId}> ({ban.ByName}) on " +
                                     $"**{ban.BannedOn}** for **{ban.Reason}**. Ban expires on **{expiryDate}**. Lifted.");
                }
                else
                    await ReplyAsync($"<@{ban.UId}> ({ban.Name}) by <@{ban.ByUId}> ({ban.ByName}) on " +
                                     $"**{ban.BannedOn}** for **{ban.Reason}**. Ban is permanent. Lifted.");

                BanningService.RemoveBan(ban.UId);
                LoggerService.Write($"{Context.User.Username}: /banlift {user} - lifted {ban.UId}");
            }

        }
    }
}
