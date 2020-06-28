using System.Linq;
using System.Threading.Tasks;
using main.Utils;
using Discord;
using Discord.Commands;
using main.Services;
using main.Core;

namespace main.Modules
{
    /// <summary>
    /// Encapsulates all banning-related commands.
    /// </summary>
    #pragma warning disable 4014,1998
    public class BanningModule : ModuleBase<SocketCommandContext>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly BanningService _banningService;
        private readonly ulong _adminChannelId;

        public BanningModule(ITimeProvider timeProvider, BanningService banningService)
        {
            _timeProvider = timeProvider;
            _banningService = banningService;
            _adminChannelId = Configuration.GetVariable("Guild.AdminChannelId");
        }

        [Command("ban")]
        public async Task Ban(
            IUser user = null, 
            int days = 0, 
            int hours = 0,
            int sendMessageToUser = 0,
            [RemainderAttribute]string reason = StringConstants.NoReasonGiven)
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
                ReplyAsync(StringConstants.UserNotFound);
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
                var expiresOn = _timeProvider.UtcNow.AddSeconds(timeToAdd);
                ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban will expire on **{expiresOn.ToHumanReadableString()}**.");
                
                if (sendMessageToUser == 1)
                    dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for **{reason}**. This ban will expire on **{expiresOn.ToHumanReadableString()}**.");
            }
            else
            {
                ReplyAsync($"Banned <@{guildUser.Id}> ({guildUser.Username}) for {reason}. Ban is permanent.");

                if (sendMessageToUser == 1)
                    dmChannel.SendMessageAsync($"You have been banned from **{Context.Guild.Name}** for **{reason}**. This ban is permanent.");
            }
            
            _banningService.StoreBan(
                guildUser.Id, 
                guildUser.Username, 
                Context.User.Id, 
                Context.User.Username, 
                timeToAdd*isTimedBan, reason
                );
            Context.Guild.AddBanAsync(guildUser, 0, $"By {Context.User.Username} for {reason}");
        }

        [Command("banlookup")]
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
                    ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn.DateTime.ToHumanReadableString()}** for **{ban.Reason}**. Ban expires on **{ban.ExpiresOn?.ToHumanReadableString()}**.");
                }
                else
                    ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn.DateTime.ToHumanReadableString()}** for **{ban.Reason}**. Ban is permanent.");
            });
        }
        
        [Command("unban")]
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

            foreach (var ban in bans.Where(b => b.Lifted == 0))
            {
                if (ban.ExpiresOn != null) 
                {
                    ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn.DateTime.ToHumanReadableString()}** for **{ban.Reason}**. Ban expires on **{ban.ExpiresOn?.ToHumanReadableString()}**. Lifted.");
                }
                else ReplyAsync($"<@{ban.Userid}> ({ban.Name}) by <@{ban.ByUserid}> ({ban.ByName}) on **{ban.BannedOn.DateTime.ToHumanReadableString()}** for **{ban.Reason}**. Ban is permanent. Lifted.");

                _banningService.RemoveBan(ban.Userid);
                Context.Guild.RemoveBanAsync(ban.Userid);
            }
        }
    }
}
