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
            if (UserService.IsUserOnCooldown(Context.User.Id, "srv"))
            {
                LoggerService.Write($"[cooldown] user {Context.User.Username} {Context.User.Id} is on a cooldown, " +
                                    "cmd blocked");
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username}: /server - {ipPort}");
            if (Context.Guild == null)
            {
                await ReplyAsync(MessageHelper.COMMAND_SERVER_ONLY);
                return;
            }

            if (Context.Channel.Id != Program.BOT_CHAN_ID && Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Context.User.SendMessageAsync($"This command only works on <#{Program.BOT_CHAN_ID}>.");
                return;
            }

            if (ipPort == "")
            {
                await ReplyAsync("/server <ip>[:port] - fetch SAMP server data");
                return;
            }

            string ip = ipPort;
            string defaultPort = "7777";
            if (ipPort.Contains(':'))
            {
                var temp = ipPort.Split(':');
                ip = temp[0];
                defaultPort = temp[1];

                LoggerService.Write($"{Context.User.Username}: /server - {ipPort} - splitted {ip} {defaultPort}");
            }

            ushort port = 7777;
            if (!UInt16.TryParse(defaultPort, out port))
            {
                LoggerService.Write($"{Context.User.Username}: /server - port not parsed");

                await ReplyAsync(NOT_VALID_SERVER);
                return;
            }

            if (!SAMPServerService.ValidateIPv4(ip) && !SAMPServerService.ValidateHostname(ip))
            {
                LoggerService.Write($"{Context.User.Username}: /server - not ip not hostname");

                await ReplyAsync(NOT_VALID_SERVER);
                return;
            }

            if (!SAMPServerService.ValidateIPv4(ip) && SAMPServerService.ValidateHostname(ip))
            {
                // not ip, but looks like a hostname. Try to get ip
                try
                {
                    IPAddress[] iPs = await Dns.GetHostAddressesAsync(ip);
                    // should be first in the DNS entry (assuming)
                    if (iPs.Length == 0)
                    {
                        LoggerService.Write($"{Context.User.Username}: /server - hostname not resolved");
                        UserService.SetUserCooldown(Context.User.Id, "srv", 8);

                        await ReplyAsync(NOT_VALID_SERVER);
                        return;
                    }
                    ip = iPs[0].ToString();

                    LoggerService.Write($"{Context.User.Username}: /server - hostname found ip {ip}");
                }
                catch (Exception)
                {
                    await ReplyAsync(FAILED_FETCH_SERVER_DATA);
                    return;
                }
            }

            SampServerResponseModel data = null;
            var generalInfo = new SAMPServerQueryService(ip, port, 'i', 1000).read();
            if (generalInfo.Count == 0)
            {
                LoggerService.Write($"{Context.User.Username}: /server - 0 generalInfo size");
                UserService.SetUserCooldown(Context.User.Id, "srv", 15);

                await ReplyAsync(FAILED_FETCH_SERVER_DATA);
                return;
            }

            var rulesInfo = new SAMPServerQueryService(ip, port, 'r', 2000).read();
            Boolean isHostedTab = await SAMPServerService.CheckGameMpWebsite(ip, port);

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
                    HostedTab = (isHostedTab) ? "1" : "0",
                    Players = generalInfo["players"],
                    MaxPlayers = generalInfo["maxplayers"]
                };
            }
            catch (Exception e)
            {
                LoggerService.Write($"{Context.User.Username} /server - key not found: {e}");
                UserService.SetUserCooldown(Context.User.Id, "srv", 15);

                await ReplyAsync(FAILED_FETCH_SERVER_DATA);
                return;
            }

            if (data == null)
            {
                LoggerService.Write($"{Context.User.Username}: /server - null response");
                UserService.SetUserCooldown(Context.User.Id, "srv", 15);

                await ReplyAsync(FAILED_FETCH_SERVER_DATA);
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
                .AddField("Hosted Tab", (data.HostedTab == "1" ? ":white_check_mark:" : ":x:"), true)
                .AddField("Online Players", $"{data.Players}/{data.MaxPlayers}", true);

            var embed = builder.Build();
            await ReplyAsync("", embed: embed);

            UserService.SetUserCooldown(Context.User.Id, "srv", 60);
        }
    }
}
