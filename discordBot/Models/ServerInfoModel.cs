using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discordBot.Services;
using System.Linq;
using discordBot.Helpers;
using Discord.WebSocket;
using System.Threading;
using System.Net;

namespace discordBot.Models
{
    public class ServerInfoModel : ModuleBase<SocketCommandContext>
    {
        // Server 
        [Command("server")]
        [Alias("srv")]
        [Name("server")]
        [Summary("/server <ip>[:port]")]
        public async Task serverAsync(string ip_port = "")
        {
            // server cmd cooldown
            if (UserService.IsUserOnCooldown(Context.User.Id, "srv"))
            {
                LoggerService.Write($"[cooldown] user {Context.User.Username} {Context.User.Id} is on a cooldown, cmd blocked");
                await Task.CompletedTask;
                return;
            }

            LoggerService.Write($"{Context.User.Username} /server - {ip_port}");

            if (Context.Guild == null)
            {
                await ReplyAsync("This only works on the server.");
                return;
            }

            if (Context.Channel.Id != Program.BOT_CHAN_ID && Context.Channel.Id != Program.ADMIN_CHAN_ID)
            {
                await Context.User.SendMessageAsync($"This command only works on <#{Program.BOT_CHAN_ID}>");
                return;
            }

            if (ip_port == "")
            {
                await ReplyAsync("/server <ip>[:port] - fetch SAMP server data");
                return;
            }

            string ip = ip_port;
            string port_ = "7777";
            if (ip_port.Contains(':'))
            {
                var split_ = ip_port.Split(':');
                ip = split_[0];
                port_ = split_[1];

                LoggerService.Write($"{Context.User.Username} /server - {ip_port} - splitted {ip} {port_}");
            }

            ushort port = 7777;
            if (!UInt16.TryParse(port_, out port))
            {
                LoggerService.Write($"{Context.User.Username} /server - port not parsed");

                await ReplyAsync("This doesn't look like a valid server port to me.");
                return;
            }

            if (!SAMPServerService.ValidateIPv4(ip) && !SAMPServerService.ValidateHostname(ip))
            {
                LoggerService.Write($"{Context.User.Username} /server - not ip not hostname");

                await ReplyAsync("This doesn't look like a valid server to me.");
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
                        LoggerService.Write($"{Context.User.Username} /server - hostname not resolved");
                        UserService.SetUserCooldown(Context.User.Id, "srv", 8);

                        await ReplyAsync("This doesn't look like a valid server to me.");
                        return;
                    }
                    ip = iPs[0].ToString();

                    LoggerService.Write($"{Context.User.Username} /server - hostname found ip {ip}");
                }
                catch (Exception)
                {
                    await ReplyAsync("Sorry! Couldn't fetch this server's data.");
                    return;
                }
            }

            /*var data = await SAMPServerService.GetInfoAsync(ip, port);*/
            SAMPServerResponseModel data = null;
            var generalInfo = new SAMPServerQueryService(ip, port, 'i', 1000).read();
            if (generalInfo.Count == 0)
            {
                LoggerService.Write($"{Context.User.Username} /server - 0 generalInfo size");
                UserService.SetUserCooldown(Context.User.Id, "srv", 15);

                await ReplyAsync("Sorry! Couldn't fetch this server's data.");
                return;
            }

            var rulesInfo = new SAMPServerQueryService(ip, port, 'r', 2000).read();
            Boolean isHostedTab = await SAMPServerService.CheckGameMpWebsite(ip, port);

            try
            {
                data = new SAMPServerResponseModel()
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

                await ReplyAsync("Sorry! Couldn't fetch this server's data.");
                return;
            }

            if (data == null)
            {
                LoggerService.Write($"{Context.User.Username} /server - null response");
                UserService.SetUserCooldown(Context.User.Id, "srv", 15);

                await ReplyAsync("Sorry! Couldn't fetch this server's data.");
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
