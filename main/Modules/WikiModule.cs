using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using main.Services;
using main.Core;
using main.Exceptions;
using main.Models;

namespace main.Modules
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
        private readonly string _wikiSearchUrl;

        public WikiModule(UserService userService, WikiService wikiService, MessageService messageService)
        {
            _userService = userService;
            _wikiService = wikiService;
            _messageService = messageService;
            _adminChannelId = Configuration.GetVariable("Guild.AdminChannelId");
            _botChannelId = Configuration.GetVariable("Guild.BotCommandsChannelId");
            _scriptingChannelId = Configuration.GetVariable("Guild.ScriptingChannelId");
            _wikiSearchUrl = Configuration.GetVariable("Urls.Wiki.Seach");
        }

        [Command("wiki")]
        public async Task Wiki(string input = "")
        {
            if (_userService.IsUserOnCooldown(Context.User.Id, "wiki"))
                return;

            if (Context.Channel.Id != _botChannelId && 
                Context.Channel.Id != _adminChannelId && 
                Context.Channel.Id != _scriptingChannelId)
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

            WikiPageData articleData;
            try
            {
                articleData = _wikiService.GetPageData(input);
            }
            catch (InvalidWikiPageException)
            {
                var response = await ReplyAsync($"Sorry! I haven't found any similar matches. Try the wiki search: <{_wikiSearchUrl}{input}>");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            StringBuilder sbArguments = new StringBuilder();
            var builder = new EmbedBuilder()
                .WithDescription("```cpp" +
                                 "\n" +
                                 $"{articleData.Title}{articleData.Arguments}```" +
                                 "\n" +
                                 $"{articleData.Description}")
                .WithColor(new Color(0xB34B00))
                .WithAuthor(author => {
                    author
                        .WithName($"SAMP Wiki - {articleData.Title}")
                        .WithUrl(articleData.Url)
                        .WithIconUrl("https://wiki.sa-mp.com/wroot/skins/common/images/wikilogo.gif");
                });

            if (articleData.ArgumentsDescriptions.Count > 0)
            {
                foreach (var argument in articleData.ArgumentsDescriptions)
                {
                    var strToAdd = $"[{argument.Key}]({articleData.Url}): {argument.Value}\n";

                    if (sbArguments.Length + strToAdd.Length <= 1020)
                        sbArguments.Append(strToAdd);
                    else
                    {
                        sbArguments.Append("...");
                        break;
                    }
                }

                builder.AddField(
                    "Parameters", 
                    sbArguments.ToString().Replace("\n...", "...")
                    );
            }

            if (articleData.CodeExample != string.Empty)
            {
                builder.AddField(
                    "Sample", 
                    "```cpp" +
                    "\n" 
                    + articleData.CodeExample +
                    "\n```");
            }

            var responseMessage = await ReplyAsync(null, embed: builder.Build());
            _messageService.LogCommand(Context.Message.Id, responseMessage.Id);

            _userService.SetUserCooldown(Context.User.Id, "wiki", 15);
        }
    }
}
