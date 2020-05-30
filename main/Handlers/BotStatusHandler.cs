using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using main.Core;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace main.Handlers
{
    #pragma warning disable 4014,1998
    public class BotStatusHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly Timer _updateTimer;
        private readonly ulong _guildId;
        private readonly ulong _adminChanId;

        public BotStatusHandler(IServiceProvider services, Configuration configuration)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _updateTimer = new Timer(this.OnPlayerCountUpdateCheck, null, 10000, 603000);
            _guildId = UInt64.Parse(configuration.GetVariable("GUILD_ID"));
            _adminChanId = UInt64.Parse(configuration.GetVariable("ADMIN_CHAN_ID"));

            Logger.Write("The SA-MP player status count has been hooked to OnPlayerCountUpdateCheck!");
        }
        
        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }
        
        private async void OnPlayerCountUpdateCheck(object state)
        {
            try
            {
                GetSampPlayerServerCount(out long playersCount, out long serversCount);
                _discord.SetGameAsync($"Grand Theft Auto San Andreas - Players Online: {playersCount} - Servers Online: {serversCount}");
            }
            catch (Exception e)
            {
                _discord.GetGuild(_guildId).GetTextChannel(_adminChanId)
                    .SendMessageAsync($"Failed to parse player count and server count due to: {e.Message}");
            }
        }

        private void GetSampPlayerServerCount(out long playersCount, out long serversCount)
        {
            playersCount = 0;
            serversCount = 0;

            var websiteContent = GetSampWebsiteContentAsync().Result;

            Match playersCountMatch = Regex.Match(websiteContent, "<td><font size=\"2\">Players Online: <\\/font><font size=\"2\" color=\"#BBBBBB\"><b>([0-9]+)<\\/b><\\/font><\\/td>");
            if (playersCountMatch.Success && websiteContent.Contains("Players Online:"))
            {
                playersCount = Int64.Parse(playersCountMatch.Groups[0].Value.Remove(0, 76).Replace("</b></font></td>", ""));
            }

            Match serversCountMatch = Regex.Match(websiteContent, "<td><font size=\"2\">Servers Online: <\\/font><font size=\"2\" color=\"#BBBBBB\"><b>([0-9]+)<\\/b><\\/font><\\/td>");
            if (serversCountMatch.Success && websiteContent.Contains("Servers Online:"))
            {
                serversCount = Int64.Parse(serversCountMatch.Groups[0].Value.Remove(0, 76).Replace("</b></font></td>", ""));
            }
        }
        
        private async Task<string> GetSampWebsiteContentAsync()
        {
            string url = "https://www.sa-mp.com/";
            string result = string.Empty;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using (HttpClient client = new HttpClient(handler))
                {
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            result = await content.ReadAsStringAsync();
                        }
                    }
                }
            }

            return result;
        }
    }
}