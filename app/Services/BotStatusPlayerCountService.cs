using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace app.Services
{
    #pragma warning disable 4014,1998
    public class BotStatusPlayerCountService
    {
        private readonly DiscordSocketClient _discord;
        private System.Threading.Timer _updateTimer;

        public BotStatusPlayerCountService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _updateTimer = new System.Threading.Timer(this.OnPlayerCountUpdateCheck, null, 10000, 603000);

            LoggerService.Write("The SA-MP player status count has been hooked to OnPlayerCountUpdateCheck!");
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
                _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID)
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

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        result = await content.ReadAsStringAsync();
                    }
                }
            }

            return result;
        }
    }
}