using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using domain;
using HttpClient = domain.HttpClient;

namespace main.Services
{
    public class StatsService
    {
        private readonly IHttpClient _httpClient;

        public StatsService(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public (int playersCount, int serversCount) GetSampPlayerServerCount()
        {
            var websiteContent = GetSampWebsiteContent();
            Match playersCountMatch = Regex.Match(
                websiteContent, 
                "<td><font size=\"2\">Players Online: <\\/font><font size=\"2\" color=\"#BBBBBB\"><b>([0-9]+)<\\/b><\\/font><\\/td>"
                );
            Match serversCountMatch = Regex.Match(
                websiteContent, 
                "<td><font size=\"2\">Servers Online: <\\/font><font size=\"2\" color=\"#BBBBBB\"><b>([0-9]+)<\\/b><\\/font><\\/td>"
                );

            return (
                (playersCountMatch.Success && websiteContent.Contains("Players Online:")) 
                    ? Int32.Parse(playersCountMatch.Groups[0].Value
                        .Remove(0, 76).Replace("</b></font></td>", "")
                    ) 
                    : 0
                , 
                (serversCountMatch.Success && websiteContent.Contains("Servers Online:")) 
                    ? Int32.Parse(serversCountMatch.Groups[0].Value
                        .Remove(0, 76).Replace("</b></font></td>", "")
                    ) 
                    : 0
                );
        }

        private string GetSampWebsiteContent() =>
            _httpClient.GetContent("https://www.sa-mp.com/");
    }
}
