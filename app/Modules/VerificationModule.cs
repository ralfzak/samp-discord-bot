using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using app.Services;
using System.Linq;
using app.Helpers;
using Discord.WebSocket;

namespace app.Modules
{
    #pragma warning disable 4014,1998
    public class VerificationModule : ModuleBase<SocketCommandContext>
    {
        [Command("verify")]
        [Name("verify")]
        [Summary("/verify <option>")]
        public async Task Verify(string option = "")
        {
            var user = Context.User;

            // Check if the user is already verified or not
            if (UserService.IsUserVerified(user.Id))
            {
                Context.Message.DeleteAsync();
                user.SendMessageAsync("Your discord account is already verified!");
                return;
            }

            if (Context.Guild != null)
            {
                Context.Message.DeleteAsync();
                user.SendMessageAsync(VerificationHelper.GetVerificationCmdDescription(user.Mention));
                return;
            }

            // Check if user is on server
            var guildUser = Context.Client.GetGuild(Program.GUILD_ID).GetUser(user.Id);
            if (guildUser == null)
            {
                ReplyAsync($"I couldn't find you on {Context.Client.GetGuild(Program.GUILD_ID).Name}. If you are on the server, try changing your status, if not, join and then try your luck verifying!");
                return;
            }

            var userVerificationState = CacheService.GetUserVerificationState(user.Id);
            switch (option.ToLower().Trim())
            {
                case "done":
                    if (userVerificationState == VERIFICATION_STATES.NONE)
                    {
                        ReplyAsync("Your verification state is not known, type `/verify` to start your verification process.");
                        return;
                    }

                    int cachedProfile = CacheService.GetUserForumId(user.Id);
                    string cachedToken = CacheService.GetUserToken(user.Id);
                    if (cachedProfile == -1 || cachedToken == "" || userVerificationState != VERIFICATION_STATES.WAITING_CONFIRM)
                    {
                        CacheService.ClearCache(user.Id);
                        ReplyAsync("Your verification process hasn't been initiated, type `/verify` to start your verification process.");
                        return;
                    }

                    string forumNameTokenized = await VerificationService.GetForumProfileIfContainsCodeAsync(cachedProfile, cachedToken);
                    if (forumNameTokenized == string.Empty)
                    {
                        UserService.SetUserCooldown(user.Id, "", 15);
                        ReplyAsync("I couldn't find the token in your profile. Make sure your profile is set to public and the token is in your biography section." +
                            "\n" +
                            ":no_entry: You are allowed to check again in 15 seconds.");
                        return;
                    }

                    // if for some reason something is fucked and the forum id is found as linked, deny process and clear everything
                    if (UserService.IsForumProfileLinked(cachedProfile))
                    {
                        CacheService.ClearCache(user.Id);
                        ReplyAsync("Sorry! This profile is already found to be linked with a discord account!");
                        return;
                    }

                    CacheService.ClearCache(user.Id);
                    UserService.StoreUserVerification(user.Id, cachedProfile, forumNameTokenized, user.Username);

                    var discordServer = Context.Client.GetGuild(Program.GUILD_ID);
                    discordServer
                        .GetTextChannel(Program.ADMIN_CHAN_ID)
                        .SendMessageAsync($"{guildUser.Mention} ({guildUser.Username}) has successfully verified to **{forumNameTokenized}** <{Program.FORUM_PROFILE_URL}{cachedProfile}>");

                    ReplyAsync(VerificationHelper.GetVerificationSuccessMessage(user.Mention, cachedProfile));

                    guildUser.AddRoleAsync(discordServer.GetRole(Program.VERIFIED_ROLE_ID));
                    break;

                case "cancel":
                    if (userVerificationState == VERIFICATION_STATES.NONE)
                    {
                        ReplyAsync("Nothing to cancel!");
                        return;
                    }
                    
                    CacheService.ClearCache(user.Id);
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
                        ReplyAsync(VerificationHelper.GetVerificationCmdDescription(user.Mention));
                        return;
                    }

                    // if param is an int
                    int profile_id = -1;
                    if (!Int32.TryParse(option, out profile_id))
                    {
                        ReplyAsync(VerificationHelper.GetVerificationCmdDescription(user.Mention));
                        return;
                    }

                    if (profile_id < 1)
                    {
                        ReplyAsync("Sorry! This doesn't look like a valid profile id to me.");
                        return;
                    }
                    
                    if (UserService.IsForumProfileLinked(profile_id))
                    {
                        CacheService.ClearCache(user.Id);
                        await ReplyAsync("This profile is already linked to a discord account!");
                        return;
                    }

                    string token = TokenService.Generate(10);

                    CacheService.SetUserVerificationState(user.Id, VERIFICATION_STATES.WAITING_CONFIRM);
                    CacheService.SetUserToken(user.Id, token);
                    CacheService.SetUserForumId(user.Id, profile_id);

                    ReplyAsync(VerificationHelper.GetVerificationWaitingMessage(user.Mention, profile_id, token));
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
                ReplyAsync("User not found.");
                return;
            }

            int profileid = -1;
            string profileName = "";
            UserService.GetUserForumProfileID(guildUser.Id, out profileid, out profileName);
            if (profileid == -1)
            {
                ReplyAsync($"{guildUser.Mention} is not verified yet.");
                return;
            }

            if (profileid == 0)
            {
                ReplyAsync($"{guildUser.Mention} is verified but not linked to a forum account, creepy eh?");
                return;
            }

            ReplyAsync($"{guildUser.Mention} is **{profileName}**: {Program.FORUM_PROFILE_URL}{profileid}");
        }

        [Command("rvwhois")]
        [Name("rvwhois")]
        [Summary("/rvwhois <forum_id/forum_name>")]
        public async Task Rvwhois(string forumInfo = "")
        {
            if (Context.Guild == null)
                return;

            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
                return;

            if (forumInfo == "")
            {
                ReplyAsync("`/rvwhois <forum_id/forum_name>` - Find what discord user is linked to a given forum account");
                return;
            }

            var userids = UserService.GetUserIDsFromForumInfo(forumInfo);
            if (userids.Length < 1)
            {
                ReplyAsync("No such user.");
                return;
            }

            foreach (var uid in userids)
            {
                int profileid = -1;
                string profileName = "";
                UserService.GetUserForumProfileID((ulong)uid, out profileid, out profileName);

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

            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
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

            if (UserService.IsUserVerified(user.Id))
            {
                ReplyAsync($"{user.Username} is already verified.");
                return;
            }

            string forumName = "";
            if ( (forumid != 0) && UserService.IsForumProfileLinked(forumid) )
            {
                ReplyAsync($"Forum ID <{Program.FORUM_PROFILE_URL}{forumid}> is already linked to a user.");
                return;
            }
            
            if (forumid != 0)
                forumName = await VerificationService.GetForumProfileNameAsync(forumid);

            UserService.StoreUserVerification(user.Id, forumid, forumName, user.Username);

            var verifiedRole = Context.Client.GetGuild( Program.GUILD_ID).GetRole(Program.VERIFIED_ROLE_ID);

            guildUser.AddRoleAsync(verifiedRole);
            ReplyAsync($"I've set {guildUser.Username} verified as commanded!");
        }

        [Command("funverify")]
        [Name("funverify")]
        [Summary("/funverify [@]<user>")]
        public async Task Funverify(IUser user = null)
        {
            LoggerService.Write($"{Context.User.Username}: /funverify ");

            if (Context.Guild == null)
                return;

            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
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

            if (!UserService.IsUserVerified(user.Id))
            {
                ReplyAsync($"{user.Username} is not verified.");
                return;
            }

            UserService.DeleteUserVerification(user.Id);

            var verifiedRole = Context.Client.GetGuild(Program.GUILD_ID).GetRole(Program.VERIFIED_ROLE_ID);

            guildUser.RemoveRoleAsync(verifiedRole);
            ReplyAsync($"I've sent {guildUser.Username} to doom!");
        }
    }
}
