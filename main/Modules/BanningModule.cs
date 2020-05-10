﻿using System;
using System.Threading.Tasks;
using main.Helpers;
using Discord;
using Discord.Commands;
using main.Services;
using main.Core;

namespace main.Modules
{
    #pragma warning disable 4014,1998
    public class BanningModule : ModuleBase<SocketCommandContext>
    {
        private readonly BanningService _banningService;
        private readonly ulong _adminChannelId;

        public BanningModule(Configuration configuration, BanningService banningService)
        {
            _banningService = banningService;
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
        }

        [Command("ban")]
        [Name("ban")]
        [Summary("/ban")]
        public async Task Ban(IUser user = null, int days = 0, int hours = 0, int sendMessageToUser = 0, [RemainderAttribute]string reason = MessageHelper.NO_REASON_GIVEN)
        {
            if (Context.Channel.Id != _adminChannelId)
                return;

            if (user == null)
            {
                ReplyAsync("`/ban [@]<user> [days] [hours] [send ban message] [reason]` - Issues a discord ban" + 
                    "\n" + 
                    "Set **days** & **hours** to 0 for a permanent ban" +
                    "\n" + 
                    "Set **send ban message** to 1 to direct message the user the ban reason and expire time, 0 not to: <https://i.imgur.com/2RUOa7L.png>");
                return;
            }

            var guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                ReplyAsync(MessageHelper.USER_NOT_FOUND);
                return;
            }

            if (days < 0 || hours < 0)
            {
                ReplyAsync("Invalid duration.");
                return;
            }

            if (sendMessageToUser != 0 && sendMessageToUser != 1)
            {
                ReplyAsync("Set send user message to either 0 or 1.");
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
                ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban will expire on {expiresOn}.");
                
                if (sendMessageToUser == 1)
                    dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for **{reason}**. This ban will expire on {expiresOn}.");
            }
            else
            {
                ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban is permanent.");

                if (sendMessageToUser == 1)
                    dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for **{reason}**. This ban is permanent.");
            }
            
            _banningService.StoreBan(guildUser.Id, guildUser.Username, Context.User.Id, Context.User.Username, timeToAdd*isTimedBan, reason);
            Context.Guild.AddBanAsync(guildUser, 0, $"{reason} by {Context.User.Username} (use /banlookup for more information");
        }

        [Command("banlookup")]
        [Name("banlookup")]
        [Summary("/banlookup")]
        public async Task Banlookup(string user = "")
        {
            if (Context.Channel.Id != _adminChannelId)
                return;

            if (user == null)
            {
                ReplyAsync("`/banlookup <userid, username>` - Search for discord issued bans" +
                    "\n" +
                    "Search by a ID (*Open the ban list, Right click, Copy ID*)");
                return;
            }

            var bans = _banningService.GetBans(user);
            if (bans.Count == 0)
            {
                ReplyAsync("No bans found.");
                return;
            }

            bans.ForEach(async ban =>
            {
                if (ban.ExpiresOn != null)
                {
                    var expiresOnDate = ban.ExpiresOn?.ToString("dddd, dd MMMM yyyy");
                    ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn}** for **{ban.Reason}**. Ban expires on **{expiresOnDate}**.");
                }
                else
                    ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn}** for **{ban.Reason}**. Ban is permanent.");
            });
        }
        
        [Command("unban")]
        [Name("unban")]
        [Summary("/unban")]
        public async Task Unban(string user = "")
        {
            if (Context.Channel.Id != _adminChannelId)
                return;

            if (user == null)
            {
                ReplyAsync("`/unban <userid, username>` - Removes a discord ban" + 
                    "\n" + 
                    "Search by a ID (*Open the ban list, Right click, Copy ID*)" +
                    "\n" +
                    "**TIP** Alternative to using this command is lifting the ban via discord UI");
                return;
            }

            var bans = _banningService.GetBans(user);
            if (bans.Count == 0)
            {
                ReplyAsync("No bans found to lift.");
                return;
            }

            foreach (var ban in bans)
            {
                if (ban.ExpiresOn != null)
                {
                    var expiryDate = ban.ExpiresOn?.ToString("dddd, dd MMMM yyyy");
                    ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn}** for **{ban.Reason}**. Ban expires on **{expiryDate}**. Lifted.");
                }
                else ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn}** for **{ban.Reason}**. Ban is permanent. Lifted.");

                _banningService.RemoveBan(ban.Userid);
                Context.Guild.RemoveBanAsync(ban.Userid);
            }
        }
    }
}
