using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using main.Services;
using main.Models;
using System.Net;
using main.Core;

namespace main.Modules
{
    #pragma warning disable 4014,1998
    public class ServerInfoModule : ModuleBase<SocketCommandContext>
    {
        private const string NotValidServer = "This doesn't look like a valid server to me :thinking:.";
        private const string FailedFetchServerData = "Sorry! I Couldn't fetch this server's data.";

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
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }

            var (ip, port) = ("localhost", (ushort)7777);
            try
            {
                (ip, port) = ParseIp(ipPort);
            }
            catch (InvalidCastException)
            {
                _userService.SetUserCooldown(Context.User.Id, "server", 5);
                var response = await ReplyAsync(NotValidServer);
                _messageService.LogCommand(Context.Message.Id, response.Id);
                return;
            }
            
            ServerResponseModel data;
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

                var response = await ReplyAsync(FailedFetchServerData);
                _messageService.LogCommand(Context.Message.Id, response.Id);
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
            _messageService.LogCommand(Context.Message.Id, responseMessage.Id);

            _userService.SetUserCooldown(Context.User.Id, "server", 60);
        }

        private (string ip, ushort port) ParseIp(string ipPort)
        {
            string ip = string.Empty;
            string _port = "7777";
            if (ipPort.Contains(':'))
            {
                var temp = ipPort.Split(':');
                if (temp.Length != 2)
                {
                    throw new InvalidOperationException("Wrong format");
                }
                ip = temp[0];
                _port = temp[1];
            }

            if (!UInt16.TryParse(_port, out ushort port))
            {
                throw new InvalidOperationException("Invalid port");
            }

            if (!ServerService.ValidateIPv4(ip) && !ServerService.ValidateHostname(ip))
            {
                throw new InvalidOperationException("Invalid IP");
            }

            if (!ServerService.ValidateIPv4(ip) && ServerService.ValidateHostname(ip))
            {
                IPAddress[] iPs = Dns.GetHostAddressesAsync(ip).Result;
                if (iPs.Length == 0)
                {
                    throw new InvalidOperationException("Failed to find DNS entry");
                }
                ip = iPs[0].ToString();
            }
            
            return (ip, port);
        }
    }
}
