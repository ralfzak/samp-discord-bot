using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using main.Services;
using main.Models;
using System.Net;
using domain.Models;
using main.Core;
using main.Exceptions;

namespace main.Modules
{
    #pragma warning disable 4014,1998
    public class ServerInfoModule : ModuleBase<SocketCommandContext>
    {
        private const string NotValidServer = "This doesn't look like a valid server to me :thinking:.";
        private const string FailedFetchServerData = "Sorry! I Couldn't fetch this server's data.";

        private readonly UserService _userService;
        private readonly MessageService _messageService;
        private readonly SampServerService _sampServerService;
        private readonly ulong _botChannelId;
        private readonly ulong _adminChannelId;

        public ServerInfoModule(
            Configuration configuration, 
            UserService userService, 
            MessageService messageService, 
            SampServerService sampServerService)
        {
            _userService = userService;
            _messageService = messageService;
            _sampServerService = sampServerService;
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
            _botChannelId = UInt64.Parse(configuration.GetVariable("BOT_CHAN_ID"));
        }

        [Command("server")]
        [Alias("srv")]
        public async Task Server(string ipPort = "")
        {
            if (_userService.IsUserOnCooldown(Context.User.Id, "server"))
                return;

            if (Context.Channel.Id != _botChannelId && Context.Channel.Id != _adminChannelId)
            {
                Context.User.SendMessageAsync($"This command only works on <#{_botChannelId}>.");
                return;
            }

            if (ipPort == "")
            {
                var response = await ReplyAsync("`/server <ip>[:port]` - Fetch a SAMP server's live data");
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            SampServerData serverData;
            try
            {
                var (ip, port) = _sampServerService.ParseIpPort(ipPort);
                serverData = _sampServerService.GetServerData(ip, port);
            }
            catch (InvalidIpParseException)
            {
                _userService.SetUserCooldown(Context.User.Id, "server", 5);
                
                var response = await ReplyAsync(NotValidServer);
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            } catch (UnableToConnectToServerException)
            {
                _userService.SetUserCooldown(Context.User.Id, "server", 10);

                var response = await ReplyAsync(FailedFetchServerData);
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            var builder = new EmbedBuilder()
                .WithDescription($"[{serverData.Url}](http://{serverData.Url})")
                .WithColor(new Color(0x1AE45))
                .WithFooter(footer => {
                    footer
                        .WithText("Live data from SAMP server")
                        .WithIconUrl("https://forum.sa-mp.com/images/samp/logo_forum.gif");
                })
                .WithAuthor(author => {
                    author
                        .WithName($"{serverData.Ip}:{serverData.Port} ({serverData.Version})");
                })
                .AddField("Hostname", serverData.Hostname, true)
                .AddField("Gamemode", serverData.Gamemode)
                .AddField("Hosted Tab", serverData.IsHostedTab ? ":white_check_mark:" : ":x:", true)
                .AddField("Online Players", $"{serverData.CurrentPlayers}/{serverData.MaxPlayers}", true);
            
            var responseMessage = await ReplyAsync("", embed: builder.Build());
            _messageService.LogCommand(Context.Message.Id, responseMessage.Id);

            _userService.SetUserCooldown(Context.User.Id, "server", 60);
        }
    }
}
