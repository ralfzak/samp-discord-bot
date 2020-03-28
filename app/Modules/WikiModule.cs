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
using System.Net;

namespace app.Modules
{
    public class WikiModule : ModuleBase<SocketCommandContext>
    {
        // Server 
        [Command("wiki")]
        [Name("wiki")]
        [Summary("/wiki")]
        public async Task Wiki(string input = "")
        {
            // server cmd cooldown
            if (UserService.IsUserOnCooldown(Context.User.Id, "wiki"))
            {
                LoggerService.Write($"[cooldown] user {Context.User.Username} {Context.User.Id} is on a cooldown, " +
                                    "cmd blocked");
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username} /wiki - {input}");

            if (Context.Guild == null)
            {
                await ReplyAsync("This only works on the server.");
                return;
            }

            if (Context.Channel.Id != Program.BOT_CHAN_ID && 
                Context.Channel.Id != Program.ADMIN_CHAN_ID && 
                Context.Channel.Id != Program.SCRIPTING_CHAN_ID)
            {
                await Context.User.SendMessageAsync($"This command only works on <#{Program.BOT_CHAN_ID}> " +
                                                    $"and <#{Program.SCRIPTING_CHAN_ID}>");
                return;
            }

            if (input == "")
            {
                await ReplyAsync("/wiki <callback/function/article> - fetch SAMP wiki article information");
                return;
            }

            string article = SAMPWikiService.GetClosestArticle(input);

            var articleInfo = await SAMPWikiService.GetInfoAsync(article);
            if (object.ReferenceEquals(articleInfo, null))
            {
                await ReplyAsync("Sorry! I haven't found any similar matches. " +
                                 $"Try the wiki search: <https://wiki.sa-mp.com/wiki/Special:Search?search={input}>");
                return;
            }

            if (articleInfo.status == "article")
            {
                await ReplyAsync($"Looks like a wiki article to me: https://wiki.sa-mp.com/wiki/{input}");
                return;
            }

            if (articleInfo.status != "ok")
            {
                await ReplyAsync("Sorry! I haven't found any similar matches. " +
                                 $"Try the wiki search: <https://wiki.sa-mp.com/wiki/Special:Search?search={input}>");
                return;
            }

            if (articleInfo.parameters == "" || articleInfo.description == "")
            {
                await ReplyAsync($"Looks like a wiki article to me: https://wiki.sa-mp.com/wiki/{input}");
                return;
            }

            // lets build our message based on what we have
            string URL = $"https://wiki.sa-mp.com/wiki/{article}";
            StringBuilder sbParam = new StringBuilder();
            StringBuilder sbExample = new StringBuilder();

            var builder = new EmbedBuilder()
                .WithDescription("```cpp" +
                    "\n" +
                    $"{article}{articleInfo.parameters}```\n{articleInfo.description}")
                .WithColor(new Color(0xB34B00))
                .WithAuthor(author => {
                    author
                        .WithName($"SAMP Wiki - {article}")
                        .WithUrl(URL)
                        .WithIconUrl("https://wiki.sa-mp.com/wroot/skins/common/images/wikilogo.gif");
                });

            if (articleInfo.param.Count() > 0)
            {
                foreach (var p in articleInfo.param)
                {
                    var strToAdd = $"[{p.name}]({URL}): {p.desc}\n";

                    if (sbParam.Length + strToAdd.Length <= 1020)
                        sbParam.Append(strToAdd);
                    else
                    {
                        sbParam.Append("...");
                        break;
                    }
                }

                builder.AddField("Parameters", sbParam.ToString().Replace("\n...", "..."));
            }

            var embed = builder.Build();
            await ReplyAsync(null, embed: embed);

            UserService.SetUserCooldown(Context.User.Id, "wiki", 15);
        }
    }
}
