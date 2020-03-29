using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using app.Services;
using System.Linq;
using app.Helpers;
using Discord.WebSocket;
using System.Threading;

namespace app.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        // Help
        [Command("help")]
        [Alias("cmds", "commands")]
        [Name("help")]
        [Summary("/help")]
        public async Task Help()
        {
            Embed embed = null;
            if (Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                LoggerService.Write($"{Context.User.Username}: /help - general");
                embed = GetPublicHelpList();

                await Context.User.SendMessageAsync("", embed: embed);
            }
            else
            {
                LoggerService.Write($"{Context.User.Username}: /help - admin");
                embed = GetPrivateHelpList();

                await ReplyAsync("", embed: embed);
            }
        }

        public static Embed GetPublicHelpList()
        {
            var builder = new EmbedBuilder()
                .WithTitle("General Commands")
                .WithDescription("List of general server commands that I respond to:")
                .WithColor(new Color(0xD0021B))
                .AddField("/verify",
                    "[**Parameters:**](https://forum.sa-mp.com/) <profile_id/done/cancel>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) Links your discord account with a SAMP forum account " +
                    "using a simple verification process." +
                    "\n" +
                    "[**Availability:**](https://forum.sa-mp.com/) As a direct message.")
                
                .AddField("/whois",
                    "[**Parameters:**](https://forum.sa-mp.com/) [@]<user>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command looks up SAMP forum profiles of given " +
                    "linked discord accounts." +
                    "\n" +
                    "[**Availability:**](https://forum.sa-mp.com/) On the discord server where I am available.")

                .AddField("/server, /srv",
                    "[**Parameters:**](https://forum.sa-mp.com/) <ip/hostname>[:port]" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command fetches SAMP server data for a given server." +
                    "\n" +
                    $"[**Availability:**](https://forum.sa-mp.com/) Only on <#{Program.BOT_CHAN_ID}>.")

            .AddField("/wiki",
                    "[**Parameters:**](https://forum.sa-mp.com/) <callback/function/article>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command fetches articles from the official SAMP Wiki." +
                    "\n" +
                    $"[**Availability:**](https://forum.sa-mp.com/) Only on <#{Program.BOT_CHAN_ID}> and " +
                    $"<#{Program.SCRIPTING_CHAN_ID}>.");

            return builder.Build();
        }

        public static Embed GetPrivateHelpList()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Admin Commands")
                .WithDescription("List of admin commands that I only respond to here:")
                .WithColor(new Color(0xD0021B))
                .AddField("/fverify",
                    "[**Parameters:**](https://forum.sa-mp.com/) [@]<user> [forumid]" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) Force links a discord account with a SAMP forum account.")

                .AddField("/funverify",
                    "[**Parameters:**](https://forum.sa-mp.com/) [@]<user>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) Drops a forum verification and removes the user role.")

                .AddField("/rvwhois",
                    "[**Parameters:**](https://forum.sa-mp.com/) <forum_id/forum_name>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command fetches discord user(s) linked to a given " +
                    "forum account.")

                .AddField("/ban",
                    "[**Parameters:**](https://forum.sa-mp.com/) [@]<user> [days] [hours] [send ban message] [reason]" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command issues a discord ban. Use this command with " +
                    "no parameters for more information. Check channel pins.")

                .AddField("/banlookup",
                    "[**Parameters:**](https://forum.sa-mp.com/) <userid, username>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command lists a list of banned account based on a " +
                    "search criteria. Check channel pins.")

                .AddField("/unban",
                    "[**Parameters:**](https://forum.sa-mp.com/) <userid, username>" +
                    "\n" +
                    "[**Info:**](https://forum.sa-mp.com/) This command lifts a list of bans given a certain criteria. " +
                    "Check channel pins.");

            return builder.Build();
        }
    }
}
