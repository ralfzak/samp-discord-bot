using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using app.Services;
using app.Models;
using System.Net;
using app.Core;

namespace app.Modules
{
    #pragma warning disable 4014,1998
    public class ServerInfoModule : ModuleBase<SocketCommandContext>
    {
        private const string NOT_VALID_SERVER = "This doesn't look like a valid server to me :thinking:.";
        private const string FAILED_FETCH_SERVER_DATA = "Sorry! I Couldn't fetch this server's data.";

        private readonly UserService _userService;
        private readonly MessageService _messageService;
        private readonly ulong _botChannelId;
        private readonly ulong _adminChannelId;

        public ServerInfoModule(Configuration configuration, UserService userService, MessageService messageService)
        {
            _userService = userService;
            _messageService = messageService;
            _adminChannelId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));
            _botChannelId = UInt64.Parse(configuration.GetVariable("BOT_CHAN_ID"));
        }

        [Command("server")]
        [Alias("srv")]
        [Name("server")]
        [Summary("/server <ip>[:port]")]
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
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            string ip = ipPort;
            string _port = "7777";
            if (ipPort.Contains(':'))
            {
                var temp = ipPort.Split(':');
                ip = temp[0];
                _port = temp[1];
            }

            ushort port = 7777;
            if (!UInt16.TryParse(_port, out port))
            {
                var response = await ReplyAsync(NOT_VALID_SERVER);
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            if (!ServerService.ValidateIPv4(ip) && !ServerService.ValidateHostname(ip))
            {
                var response = await ReplyAsync(NOT_VALID_SERVER);
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            if (!ServerService.ValidateIPv4(ip) && ServerService.ValidateHostname(ip))
            {
                // not ip, but looks like a hostname, query dns
                try
                {
                    IPAddress[] iPs = await Dns.GetHostAddressesAsync(ip);
                    if (iPs.Length == 0)
                    {
                        Logger.Write($"{Context.User.Username}: /server - hostname not resolved");
                        _userService.SetUserCooldown(Context.User.Id, "server", 8);

                        var response = await ReplyAsync(NOT_VALID_SERVER);
                        _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                        return;
                    }
                    ip = iPs[0].ToString(); // should be first in the DNS entry
                }
                catch (Exception)
                {
                    var response = await ReplyAsync(FAILED_FETCH_SERVER_DATA);
                    _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                    return;
                }
            }

            ServerResponseModel data = null;
            var generalInfo = new ServerQueryService(ip, port, 'i', 3000).read();
            var rulesInfo = new ServerQueryService(ip, port, 'r', 3000).read();
            var isHostedTab = await ServerService.CheckGameMpWebsite(ip, port);

            try
            {
                data = new ServerResponseModel()
                {
                    WebURL = rulesInfo["weburl"],
                    IP = ip,
                    Port = port.ToString(),
                    Version = rulesInfo["version"],
                    Hostname = generalInfo["hostname"],
                    Gamemode = generalInfo["gamemode"],
                    HostedTab = isHostedTab,
                    Players = generalInfo["players"],
                    MaxPlayers = generalInfo["maxplayers"]
                };
            }
            catch (Exception e)
            {
                Logger.Write($"{Context.User.Username}: /server - key not found: {e}");
                _userService.SetUserCooldown(Context.User.Id, "server", 15);

                var response = await ReplyAsync(FAILED_FETCH_SERVER_DATA);
                _messageService.LogCommand(Context.Message.Id, response.Id, Context.User.Id);
                return;
            }

            var builder = new EmbedBuilder()
                .WithDescription($"[{data.WebURL}](http://{data.WebURL})")
                .WithColor(new Color(0x1AE45))
                .WithFooter(footer => {
                    footer
                        .WithText("Live data from SAMP server")
                        .WithIconUrl("https://forum.sa-mp.com/images/samp/logo_forum.gif");
                })
                .WithAuthor(author => {
                    author
                        .WithName($"{data.IP}:{data.Port} ({data.Version})");
                })
                .AddField("Hostname", data.Hostname, true)
                .AddField("Gamemode", data.Gamemode)
                .AddField("Hosted Tab", (data.HostedTab ? ":white_check_mark:" : ":x:"), true)
                .AddField("Online Players", $"{data.Players}/{data.MaxPlayers}", true);

            var embed = builder.Build();

            var responseMessage = await ReplyAsync("", embed: embed);
            _messageService.LogCommand(Context.Message.Id, responseMessage.Id, Context.User.Id);

            _userService.SetUserCooldown(Context.User.Id, "server", 60);
        }
    }
}
