using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using app.Services;
using app.Models;
using System.Net;
using app.Helpers;

namespace app.Modules
{
    #pragma warning disable 4014,1998
    public class ServerInfoModule : ModuleBase<SocketCommandContext>
    {
        private const string NOT_VALID_SERVER = "This doesn't look like a valid server to me :thinking:.";
        private const string FAILED_FETCH_SERVER_DATA = "Sorry! I Couldn't fetch this server's data.";
        
        [Command("server")]
        [Alias("srv")]
        [Name("server")]
        [Summary("/server <ip>[:port]")]
        public async Task Server(string ipPort = "")
        {
            if (UserService.IsUserOnCooldown(Context.User.Id, "server"))
                return;

            if (Context.Channel.Id != Program.BOT_CHAN_ID && Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                Context.User.SendMessageAsync($"This command only works on <#{Program.BOT_CHAN_ID}>.");
                return;
            }

            if (ipPort == "")
            {
                ReplyAsync("`/server <ip>[:port]` - Fetch a SAMP server's live data");
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
                ReplyAsync(NOT_VALID_SERVER);
                return;
            }

            if (!SAMPServerService.ValidateIPv4(ip) && !SAMPServerService.ValidateHostname(ip))
            {
                ReplyAsync(NOT_VALID_SERVER);
                return;
            }

            if (!SAMPServerService.ValidateIPv4(ip) && SAMPServerService.ValidateHostname(ip))
            {
                // not ip, but looks like a hostname, query dns
                try
                {
                    IPAddress[] iPs = await Dns.GetHostAddressesAsync(ip);
                    if (iPs.Length == 0)
                    {
                        LoggerService.Write($"{Context.User.Username}: /server - hostname not resolved");
                        UserService.SetUserCooldown(Context.User.Id, "server", 8);

                        ReplyAsync(NOT_VALID_SERVER);
                        return;
                    }
                    ip = iPs[0].ToString(); // should be first in the DNS entry
                }
                catch (Exception)
                {
                    ReplyAsync(FAILED_FETCH_SERVER_DATA);
                    return;
                }
            }

            SampServerResponseModel data = null;
            var generalInfo = new SAMPServerQueryService(ip, port, 'i', 1000).read();
            var rulesInfo = new SAMPServerQueryService(ip, port, 'r', 2000).read();
            var isHostedTab = await SAMPServerService.CheckGameMpWebsite(ip, port);

            try
            {
                data = new SampServerResponseModel()
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
                LoggerService.Write($"{Context.User.Username}: /server - key not found: {e}");
                UserService.SetUserCooldown(Context.User.Id, "server", 15);

                ReplyAsync(FAILED_FETCH_SERVER_DATA);
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
            ReplyAsync("", embed: embed);

            UserService.SetUserCooldown(Context.User.Id, "server", 60);
        }
    }
}
