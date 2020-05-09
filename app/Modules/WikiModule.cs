using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using app.Services;
using System.Linq;
using app.Core;

namespace app.Modules
{
    #pragma warning disable 4014,1998
    public class WikiModule : ModuleBase<SocketCommandContext>
    {
        private readonly UserService _userService;
        private readonly WikiService _wikiService;
        private readonly MessageService _messageService;
        private readonly ulong _botChannelId;
        private readonly ulong _adminChannelId;
        private readonly ulong _scriptingChannelId;

        public WikiModule(Configuration configuration, UserService userService, WikiService wikiService, MessageService messageService)
        {
            _userService = userService;
            _wikiService = wikiService;
            _messageService = messageService;
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
            _botChannelId = UInt64.Parse(configuration.GetVariable("BOT_CHAN_ID"));
            _scriptingChannelId = UInt64.Parse(configuration.GetVariable("SCRIPTING_CHAN_ID"));
        }

        [Command("wiki")]
        [Name("wiki")]
        [Summary("/wiki")]
        public async Task Wiki(string input = "")
        {
            if (_userService.IsUserOnCooldown(Context.User.Id, "wiki"))
                return;

            if (Context.Channel.Id != _botChannelId && Context.Channel.Id != _adminChannelId && Context.Channel.Id != _scriptingChannelId)
            {
                Context.User.SendMessageAsync($"This command only works on <#{_botChannelId}> and <#{_scriptingChannelId}>");
                return;
            }

            if (input == "")
            {
                var response = await ReplyAsync("`/wiki <callback/function/article>` - Fetch SAMP wiki article information");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            var article = _wikiService.GetClosestArticle(input);
            var articleInfo = await _wikiService.GetInfoAsync(article);

            if (ReferenceEquals(articleInfo, null))
            {
                var response = await ReplyAsync("Sorry! I haven't found any similar matches. Try the wiki search: <https://wiki.sa-mp.com/wiki/Special:Search?search={input}>");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            if (articleInfo.status == "article")
            {
                var response = await ReplyAsync($"Looks like a wiki article to me: https://wiki.sa-mp.com/wiki/{input}");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            if (articleInfo.status != "ok")
            {
                var response = await ReplyAsync("Sorry! I haven't found any similar matches. Try the wiki search: <https://wiki.sa-mp.com/wiki/Special:Search?search={input}>");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            if (articleInfo.parameters == "" || articleInfo.description == "")
            {
                var response = await ReplyAsync($"Looks like a wiki article to me: https://wiki.sa-mp.com/wiki/{input}");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            string URL = $"https://wiki.sa-mp.com/wiki/{article}";
            StringBuilder sbParam = new StringBuilder();
            StringBuilder sbExample = new StringBuilder();

            var builder = new EmbedBuilder()
                .WithDescription(
                    "```cpp" +
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
            var responseMessage = await ReplyAsync(null, embed: embed);
            _messageService.LogCommand(Context.Message.Id, responseMessage.Id);

            _userService.SetUserCooldown(Context.User.Id, "wiki", 15);
        }
    }
}
