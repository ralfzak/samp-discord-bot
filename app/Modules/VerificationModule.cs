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
    public class VerificationModule : ModuleBase<SocketCommandContext>
    {
        // general cmds
        [Command("verify")]
        [Name("verify")]
        [Summary("/verify <option>")]
        public async Task Verify(string option = "")
        {
            var user = Context.User;
            
            if (Context.Guild != null)
            {
                if (UserService.IsUserVerified(user.Id))
                {
                    LoggerService.Write($"{user.Username}: /verify {option} " +
                                        "- server cmd & discord account already verified");
                    await Task.CompletedTask;
                    return;
                }

                await user.SendMessageAsync(VerificationHelper.GetVerificationCmdDescription(user.Mention));
                return;
            }

            LoggerService.Write($"{user.Username}: /verify {option}");

            // Check if user is on server
            var guildUser = Context.Client.GetGuild(Program.GUILD_ID).GetUser(user.Id);
            if (guildUser == null)
            {
                LoggerService.Write($"{user.Username}: /verify {option} - cannot find user on server");

                await ReplyAsync("I couldn't find you on the SA:MP discord server. If you are on the server, " +
                                 "try modifying your status, if not, join and then try your luck verifying!");
                return;
            }
            
            // Check if the user is already verified or not
            var userVerifiedInDb = UserService.IsUserVerified(user.Id);
            if (userVerifiedInDb)
            {
                LoggerService.Write($"{user.Username}: /verify {option} - discord account already verified");
                await ReplyAsync("Your discord account is already verified!");
                return;
            }

            var userVerificationState = CacheService.GetUserVerificationState(user.Id);
            var userHasVerifiedRole = (guildUser.Roles.FirstOrDefault(r => r.Id == Program.VERIFIED_ROLE_ID) != null);
            
            if (userHasVerifiedRole && (userVerificationState == VERIFICATION_STATES.NONE))
            {
                LoggerService.Write($"{user.Username} has role but is not found in DB");
                await ReplyAsync("Seems that your account data is missing, verify yourself again to fix it!");
            }

            switch (option.ToLower().Trim())
            {
                case "done":
                    if (userVerificationState == VERIFICATION_STATES.NONE)
                    {
                        LoggerService.Write($"{user.Username}: /verify {option} - state == NONE");

                        await ReplyAsync("Your verification state is not known, " +
                                         "type `/verify` to start your verification process.");
                        return;
                    }

                    if (userVerificationState != VERIFICATION_STATES.WAITING_CONFIRM)
                    {
                        LoggerService.Write($"{user.Username}: /verify {option} - state != WAITING");

                        await ReplyAsync("Your verification process hasn't been initiated, " +
                                         "type `/verify` to start your verification process.");
                        return;
                    }

                    int cachedProfile = CacheService.GetUserForumId(user.Id);
                    string cachedToken = CacheService.GetUserToken(user.Id);

                    if (cachedProfile == -1 || cachedToken == "")
                    {
                        CacheService.ClearCache(user.Id);
                        await ReplyAsync("Your verification process hasn't been initiated, " +
                                         "type `/verify` to start your verification process.");
                        return;
                    }

                    LoggerService.Write($"{user.Username}: /verify {option} " +
                                        $"- found in cache {cachedProfile} {cachedToken}");

                    string forumNameTokenized = 
                        await VerificationService.GetForumProfileIfContainsCodeAsync(cachedProfile, cachedToken);

                    LoggerService.Write($"{user.Username}: /verify {option} " +
                                        $"- scanned forum profile token found {forumNameTokenized}");
                    
                    if (forumNameTokenized == string.Empty)
                    {
                        UserService.SetUserCooldown(user.Id, "", 15);
                        await ReplyAsync("I couldn't find the token in your profile. " +
                                         "Make sure your profile is set to public and the token is in your " +
                                         "biography section." +
                                         "\n" +
                                         "You are allowed to check again in 15 seconds.");
                        return;
                    }

                    // if for some reason something is fucked and the forum id is found as linked,
                    // deny process and clear everything
                    if (UserService.IsForumProfileLinked(cachedProfile))
                    {
                        LoggerService.Write($"{user.Username}: /verify {option} - forum id already linked");

                        await ReplyAsync("Sorry! This profile is already found to be linked with a discord account!");
                        CacheService.ClearCache(user.Id);
                        return;
                    }

                    CacheService.ClearCache(user.Id);
                    UserService.StoreUserVerification(user.Id, cachedProfile, forumNameTokenized, user.Username);

                    var discordServer = Context.Client.GetGuild(Program.GUILD_ID);
                    var verifiedRole = discordServer.GetRole(Program.VERIFIED_ROLE_ID);
                    await guildUser.AddRoleAsync(verifiedRole);

                    await ReplyAsync(VerificationHelper.GetVerificationSuccessMessage(user.Mention, cachedProfile));

                    LoggerService.Write($"{user.Username}: /verify {option} - verified user {guildUser.Username} " +
                                        $"given role {verifiedRole.Name} on guild {discordServer.Name}");

                    await discordServer
                        .GetTextChannel(Program.ADMIN_CHAN_ID)
                        .SendMessageAsync($"{guildUser.Mention} ({guildUser.Username}) has successfully verified to " +
                                          $"**{forumNameTokenized}** <{Program.FORUM_PROFILE_URL}{cachedProfile}>");

                    break;

                case "cancel":
                    if (userVerificationState == VERIFICATION_STATES.NONE)
                    {
                        LoggerService.Write($"{user.Username}: /verify {option} - nothing to cancel, state = NONE");
                        await ReplyAsync("Nothing to cancel!");
                        return;
                    }
                    
                    CacheService.ClearCache(user.Id);
                    await ReplyAsync(
                        "Cleared everything. " +
                        "You can start the verification process again by typing `/verify <profile id>`");
                    
                    LoggerService.Write($"{user.Username}: /verify {option} - cache cleared");
                    break;

                default:
                    if (userVerificationState != VERIFICATION_STATES.NONE)
                    {
                        LoggerService.Write(
                            $"{user.Username}: /verify {option} - VERIFIECATION != NONE " +
                            $"({userVerificationState.ToString()})");

                        await ReplyAsync("Your verification is awaiting you to place the token in your profile biography. " +
                                         "`/verify done` once done or `/verify cancel` to cancel the process.");
                        return;
                    }

                    if (option == string.Empty)
                    {
                        LoggerService.Write($"{user.Username} /verify {option} - no profile link given");
                        
                        await ReplyAsync(VerificationHelper.GetVerificationCmdDescription(user.Mention));
                        return;
                    }

                    // if param is an int
                    int profile_id = -1;
                    if (!Int32.TryParse(option, out profile_id))
                    {
                        LoggerService.Write($"{user.Username} /verify {option} - invalid profile id");

                        await ReplyAsync(VerificationHelper.GetVerificationCmdDescription(user.Mention));
                        return;
                    }

                    if (profile_id < 1)
                    {
                        LoggerService.Write($"{user.Username} /verify {option} - invalid profile id");

                        await ReplyAsync("Sorry! This doesn't look like a valid profile id to me.");
                        return;
                    }

                    // check if a profile is already linked to an account...
                    if (UserService.IsForumProfileLinked(profile_id))
                    {
                        LoggerService.Write($"{user.Username} /verify {option} - forum id already linked");

                        await ReplyAsync("This profile is already linked with a discord account!");
                        CacheService.ClearCache(user.Id);
                        return;
                    }

                    string token = TokenService.Generate(10);

                    CacheService.SetUserVerificationState(user.Id, VERIFICATION_STATES.WAITING_CONFIRM);
                    CacheService.SetUserToken(user.Id, token);
                    CacheService.SetUserForumId(user.Id, profile_id);

                    LoggerService.Write($"{user.Username} /verify {option} - verify started " +
                                        $"token: {token} fid: {profile_id} uid: {user.Id} {user.Username}");

                    await ReplyAsync(VerificationHelper.GetVerificationWaitingMessage(user.Mention, profile_id, token));
                    break;
            }
        }

        [Command("whois")]
        [Name("whois")]
        [Summary("/whois [@]<user>")]
        public async Task Whois(IUser user = null)
        {
            user = user ?? Context.User;

            LoggerService.Write($"{Context.User.Username}: /whois {user.Username} ");

            // this is a dm
            if (Context.Guild == null)
            {
                LoggerService.Write($"{Context.User.Username}: /whois {user.Username} - as a dm");

                await ReplyAsync("This only works on the server.");
                return;
            }

            var guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                LoggerService.Write($"{Context.User.Username}: /whois {user.Username} - not found");

                await ReplyAsync("User not found.");
                return;
            }

            int profileid = -1;
            string profileName = "";
            UserService.GetUserForumProfileID(guildUser.Id, out profileid, out profileName);
            if (profileid == -1)
            {
                LoggerService.Write($"{Context.User.Username}: /whois {user.Username} - not verified");

                await ReplyAsync($"{guildUser.Mention} not verified yet.");
                return;
            }

            if (profileid == 0)
            {
                LoggerService.Write($"{Context.User.Username}: /whois {user.Username} - not linked");

                await ReplyAsync($"{guildUser.Mention} is verified but not linked to a forum account.");
                return;
            }

            await ReplyAsync($"{guildUser.Mention} is **{profileName}**: {Program.FORUM_PROFILE_URL}{profileid}");
        }

        [Command("rvwhois")]
        [Name("rvwhois")]
        [Summary("/rvwhois <forum_id/forum_name>")]
        public async Task Rvwhois(string forumInfo = "")
        {
            LoggerService.Write($"{Context.User.Username}: /rvwhois {forumInfo}");

            // this is a dm
            if (Context.Guild == null)
            {
                await Task.CompletedTask;
                return;
            }

            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Task.CompletedTask;
                return;
            }

            var userids = UserService.GetUserIDsFromForumInfo(forumInfo);
            if (userids.Length < 1)
            {
                await ReplyAsync("No such user.");
                return;
            }

            foreach (var uid in userids)
            {
                int profileid = -1;
                string profileName = "";
                UserService.GetUserForumProfileID((ulong)uid, out profileid, out profileName);

                await ReplyAsync($"**{profileName}** ({Program.FORUM_PROFILE_URL}{profileid}) is <@{uid}>");
            }
        }

        // admin cmds
        [Command("fverify")]
        [Name("fverify")]
        [Summary("/fverify [@]<user> [forumid]")]
        public async Task Fverify(IUser user = null, int forumid = 0)
        {
            // this is a dm
            if (Context.Guild == null)
            {
                LoggerService.Write($"{Context.User.Username}: /fverify ... {forumid} - as a dm");

                await Task.CompletedTask;
                return;
            }

            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                LoggerService.Write($"{Context.User.Username}: /fverify ... {forumid} - not on adm chan");

                await Task.CompletedTask;
                return;
            }

            if (user == null)
            {
                await ReplyAsync("/fverify <@user> [forumid] - force verifies a user");
                return;
            }

            LoggerService.Write($"{Context.User.Username}: /fverify {user.Username} {forumid}");

            SocketGuildUser guildUser = null;
            guildUser = Context.Guild.GetUser(user.Id);

            if (guildUser == null)
            {
                await ReplyAsync("Something went wrong, I cannot find this user!");
                return;
            }

            if (UserService.IsUserVerified(user.Id))
            {
                await ReplyAsync($"{user.Username} is already verified.");
                return;
            }

            string forumName = "";
            if ( (forumid != 0) && UserService.IsForumProfileLinked(forumid) )
            {
                await ReplyAsync($"Forum ID <{Program.FORUM_PROFILE_URL}{forumid}> is already linked to a user.");
                return;
            }
            
            if (forumid != 0)
                forumName = await VerificationService.GetForumProfileNameAsync(forumid);

            UserService.StoreUserVerification(user.Id, forumid, forumName, user.Username);

            var verifiedRole = Context.Client.GetGuild( Program.GUILD_ID).GetRole(Program.VERIFIED_ROLE_ID);
            await guildUser.AddRoleAsync(verifiedRole);

            await ReplyAsync($"I've set {guildUser.Username} verified as commanded!");
        }

        [Command("funverify")]
        [Name("funverify")]
        [Summary("/funverify [@]<user>")]
        public async Task Funverify(IUser user = null)
        {
            LoggerService.Write($"{Context.User.Username}: /funverify ");

            // this is a dm
            if (Context.Guild == null)
            {
                LoggerService.Write($"{Context.User.Username}: /funverify - as a dm");

                await Task.CompletedTask;
                return;
            }

            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                LoggerService.Write($"{Context.User.Username}: /funverify - not on adm chan");

                await Task.CompletedTask;
                return;
            }

            if (user == null)
            {
                await ReplyAsync("!funverify <@user> - force unverifies a user");
                return;
            }

            LoggerService.Write($"{Context.User.Username}: /funverify {user.Username} ");

            SocketGuildUser guildUser = null;
            guildUser = Context.Guild.GetUser(user.Id);
            if (guildUser == null)
            {
                await ReplyAsync("Something went wrong, I cannot find this user!");
                return;
            }

            if (!UserService.IsUserVerified(user.Id))
            {
                await ReplyAsync($"{user.Username} is not verified.");
                return;
            }

            UserService.DeleteUserVerification(user.Id);

            var verifiedRole = Context.Client.GetGuild(Program.GUILD_ID).GetRole(Program.VERIFIED_ROLE_ID);
            await guildUser.RemoveRoleAsync(verifiedRole);

            await ReplyAsync($"I've sent {guildUser.Username} to doom!");
        }
    }
}
