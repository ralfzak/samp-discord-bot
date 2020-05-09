using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using app.Services;
using app.Helpers;
using Discord.WebSocket;
using app.Core;

namespace app.Modules
{
    #pragma warning disable 4014,1998
    public class VerificationModule : ModuleBase<SocketCommandContext>
    {
        private readonly UserService _userService;
        private readonly CacheService _cacheService;
        private readonly VerificationService _verificationService;
        private readonly MessageService _messageService;
        private readonly ulong _guildId;
        private readonly ulong _myId;
        private readonly ulong _adminChannelId;
        private readonly ulong _verifiedRoleId;

        public VerificationModule(Configuration configuration, UserService userService, CacheService cacheService, VerificationService verificationService, MessageService messageService)
        {
            _userService = userService;
            _cacheService = cacheService;
            _messageService = messageService;
            _verificationService = verificationService;
            _guildId = UInt64.Parse(configuration.GetVariable("GUILD_ID"));
            _myId = UInt64.Parse(configuration.GetVariable("MY_ID"));
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
            _verifiedRoleId = UInt64.Parse(configuration.GetVariable("VERIFIED_ROLE_ID"));
        }

        [Command("verify")]
        [Name("verify")]
        [Summary("/verify <option>")]
        public async Task Verify(string option = "")
        {
            var user = Context.User;

            if (_userService.IsUserVerified(user.Id))
            {
                Context.Message.DeleteAsync();
                user.SendMessageAsync("Your discord account is already verified!");
                return;
            }

            if (Context.Guild != null)
            {
                Context.Message.DeleteAsync();
                user.SendMessageAsync(MessageHelper.GetVerificationCmdDescription(user.Mention));
                return;
            }

            var guildUser = Context.Client.GetGuild(_guildId).GetUser(user.Id);
            if (guildUser == null)
            {
                ReplyAsync($"I couldn't find you on {Context.Client.GetGuild(_guildId).Name}. If you are on the server, try changing your status, if not, join and then try your luck verifying!");
                return;
            }

            var userVerificationState = _cacheService.GetUserVerificationState(user.Id);
            switch (option.ToLower().Trim())
            {
                case "done":
                    if (userVerificationState == VERIFICATION_STATES.NONE)
                    {
                        ReplyAsync("Your verification state is not known, type `/verify` to start your verification process.");
                        return;
                    }

                    int cachedProfile = _cacheService.GetUserForumId(user.Id);
                    string cachedToken = _cacheService.GetUserToken(user.Id);
                    if (cachedProfile == -1 || cachedToken == "" || userVerificationState != VERIFICATION_STATES.WAITING_CONFIRM)
                    {
                        _cacheService.ClearCache(user.Id);
                        ReplyAsync("Your verification process hasn't been initiated, type `/verify` to start your verification process.");
                        return;
                    }

                    string forumNameTokenized = await _verificationService.GetForumProfileIfContainsCodeAsync(cachedProfile, cachedToken);
                    if (forumNameTokenized == string.Empty)
                    {
                        _userService.SetUserCooldown(user.Id, "", 15);
                        ReplyAsync("I couldn't find the token in your profile. Make sure your profile is set to public and the token is in your biography section." +
                            "\n" +
                            ":no_entry: You are allowed to check again in 15 seconds.");
                        return;
                    }

                    // if for some reason something is fucked and the forum id is found as linked, deny process and clear everything
                    if (_userService.IsForumProfileLinked(cachedProfile))
                    {
                        _cacheService.ClearCache(user.Id);
                        ReplyAsync("Sorry! This profile is already found to be linked with a discord account!");
                        return;
                    }

                    _cacheService.ClearCache(user.Id);
                    _userService.StoreUserVerification(user.Id, cachedProfile, forumNameTokenized, user.Username);

                    var discordServer = Context.Client.GetGuild(_guildId);
                    discordServer
                        .GetTextChannel(_adminChannelId)
                        .SendMessageAsync($"{guildUser.Mention} ({guildUser.Username}) has successfully verified to **{forumNameTokenized}** <{Program.FORUM_PROFILE_URL}{cachedProfile}>");

                    ReplyAsync(MessageHelper.GetVerificationSuccessMessage(user.Mention, cachedProfile));

                    guildUser.AddRoleAsync(discordServer.GetRole(_verifiedRoleId));
                    break;

                case "cancel":
                    if (userVerificationState == VERIFICATION_STATES.NONE)
                    {
                        ReplyAsync("Nothing to cancel!");
                        return;
                    }
                    
                    _cacheService.ClearCache(user.Id);
                    ReplyAsync("Session successfully cleared. You can start the verification process again by typing `/verify <profile id>`");
                    break;

                default:
                    if (userVerificationState != VERIFICATION_STATES.NONE)
                    {
                        ReplyAsync("Your verification is awaiting you to place the token in your profile biography. `/verify done` once done or `/verify cancel` to cancel the process.");
                        return;
                    }

                    if (option == string.Empty)
                    {
                        ReplyAsync(MessageHelper.GetVerificationCmdDescription(user.Mention));
                        return;
                    }

                    int profile_id = -1;
                    if (!Int32.TryParse(option, out profile_id))
                    {
                        ReplyAsync(MessageHelper.GetVerificationCmdDescription(user.Mention));
                        return;
                    }

                    if (profile_id < 1)
                    {
                        ReplyAsync("Sorry! This doesn't look like a valid profile id to me.");
                        return;
                    }
                    
                    if (_userService.IsForumProfileLinked(profile_id))
                    {
                        _cacheService.ClearCache(user.Id);
                        await ReplyAsync("This profile is already linked to a discord account!");
                        return;
                    }

                    string token = TokenService.Generate(10);

                    _cacheService.SetUserVerificationState(user.Id, VERIFICATION_STATES.WAITING_CONFIRM);
                    _cacheService.SetUserToken(user.Id, token);
                    _cacheService.SetUserForumId(user.Id, profile_id);

                    ReplyAsync(MessageHelper.GetVerificationWaitingMessage(user.Mention, profile_id, token));
                    break;
            }
        }

        [Command("whois")]
        [Name("whois")]
        [Summary("/whois [@]<user>")]
        public async Task Whois(IUser user = null)
        {
            user = user ?? Context.User;

            if (Context.Guild == null)
            {
                ReplyAsync(MessageHelper.COMMAND_SERVER_ONLY);
                return;
            }

            var guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                var response = await ReplyAsync("User not found.");
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            if (user.Id == _myId) // me
            {
                var response = await ReplyAsync($"{user.Mention}(穆伍兹) also known as Woozi is the blind leader of the Mountain Cloud Boys.");
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            int profileid = -1;
            string profileName = "";
            _userService.GetUserForumProfileID(guildUser.Id, out profileid, out profileName);
            if (profileid == -1)
            {
                var response = await ReplyAsync($"{guildUser.Mention} is not verified yet.");
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            if (profileid == 0)
            {
                var response = await ReplyAsync($"{guildUser.Mention} is verified but not linked to a forum account, creepy eh?");
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            var responseMessage = await ReplyAsync($"{guildUser.Mention} is **{profileName}**: {Program.FORUM_PROFILE_URL}{profileid}");
            _messageService.LogCommand(Context.Message.Id, responseMessage.Id, Context.User.Id);
        }

        [Command("rvwhois")]
        [Name("rvwhois")]
        [Summary("/rvwhois <forum_id/forum_name>")]
        public async Task Rvwhois(string forumInfo = "")
        {
            if (Context.Guild == null)
                return;

            if (Context.Channel.Id != _adminChannelId)
                return;

            if (forumInfo == "")
            {
                ReplyAsync("`/rvwhois <forum_id/forum_name>` - Find what discord user is linked to a given forum account");
                return;
            }

            var userids = _userService.GetUserIDsFromForumInfo(forumInfo);
            if (userids.Length < 1)
            {
                ReplyAsync("No such user.");
                return;
            }

            foreach (var uid in userids)
            {
                int profileid = -1;
                string profileName = "";
                _userService.GetUserForumProfileID((ulong)uid, out profileid, out profileName);

                ReplyAsync($"**{profileName}** ({Program.FORUM_PROFILE_URL}{profileid}) is <@{uid}>");
            }
        }
        
        [Command("fverify")]
        [Name("fverify")]
        [Summary("/fverify [@]<user> [forumid]")]
        public async Task Fverify(IUser user = null, int forumid = 0)
        {
            if (Context.Guild == null)
                return;

            if (Context.Channel.Id != _adminChannelId)
                return;

            if (user == null)
            {
                ReplyAsync("`/fverify <@user> [forumid]` - Force verifies a user");
                return;
            }
            
            SocketGuildUser guildUser = null;
            guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                ReplyAsync("Something went wrong, I could not find this user!");
                return;
            }

            if (_userService.IsUserVerified(user.Id))
            {
                ReplyAsync($"{user.Username} is already verified.");
                return;
            }

            string forumName = "";
            if ( (forumid != 0) && _userService.IsForumProfileLinked(forumid) )
            {
                ReplyAsync($"Forum ID <{Program.FORUM_PROFILE_URL}{forumid}> is already linked to a user.");
                return;
            }
            
            if (forumid != 0)
                forumName = await _verificationService.GetForumProfileNameAsync(forumid);

            _userService.StoreUserVerification(user.Id, forumid, forumName, user.Username);

            var verifiedRole = Context.Client.GetGuild(_guildId).GetRole(_verifiedRoleId);
            guildUser.AddRoleAsync(verifiedRole);

            ReplyAsync($"I've set {guildUser.Username} verified as commanded!");
        }

        [Command("funverify")]
        [Name("funverify")]
        [Summary("/funverify [@]<user>")]
        public async Task Funverify(IUser user = null)
        {
            if (Context.Guild == null)
                return;

            if (Context.Channel.Id != _adminChannelId)
                return;

            if (user == null)
            {
                ReplyAsync("`/funverify <@user>` - Force unverifies a user");
                return;
            }
            
            SocketGuildUser guildUser = null;
            guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                ReplyAsync("Something went wrong, I cannot find this user!");
                return;
            }

            if (!_userService.IsUserVerified(user.Id))
            {
                ReplyAsync($"{user.Username} is not verified.");
                return;
            }

            _userService.DeleteUserVerification(user.Id);

            var verifiedRole = Context.Client.GetGuild(_guildId).GetRole(_verifiedRoleId);
            guildUser.RemoveRoleAsync(verifiedRole);

            ReplyAsync($"I've sent {guildUser.Username} to doom!");
        }
    }
}
