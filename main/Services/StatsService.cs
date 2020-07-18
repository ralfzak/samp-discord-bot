using System;
using System.Text.RegularExpressions;
using main.Core;

namespace main.Services
{
    public class StatsService
    {
        private readonly IHttpClient _httpClient;
        private readonly string _sampUrl;
        
        public StatsService(IHttpClient httpClient)
        {
            _httpClient = httpClient;
            _sampUrl = Configuration.GetVariable(ConfigurationKeys.UrlSampWebsite);
        }
        
        /// <summary>
        /// Queries SAMP's official website to retrieve global player count and server count.
        /// If querying fails, either failed values are returned as 0.
        /// </summary>
        /// <returns>A global player and server count pair</returns>
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
            _httpClient.GetContent(_sampUrl);
    }
}
