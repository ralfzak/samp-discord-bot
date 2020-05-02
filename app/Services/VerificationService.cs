using System;
using app.Core;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace app.Services
{
    public enum VERIFICATION_STATES
    {
        NONE = 0,
        WAITING_CONFIRM = 1
    }

    public class VerificationService
    {
        public async Task<string> GetForumProfileContentAsync(int profileID)
        {
            string url = $"{Program.FORUM_PROFILE_URL}{profileID}";
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

        public async Task<string> GetForumProfileIfContainsCodeAsync(int profile_id, string token)
        {
            string profile_page = await GetForumProfileContentAsync(profile_id);
            Match match = Regex.Match(profile_page, @"<title>SA-MP Forums - View Profile: (.*)</title>");

            if (match.Success && profile_page.Contains(token))
                return match.Groups[0].Value.Remove(0, 36).Replace("</title>", "");

            return string.Empty;
        }

        public async Task<string> GetForumProfileNameAsync(int profile_id)
        {
            string profile_page = await GetForumProfileContentAsync(profile_id);
            Match match = Regex.Match(profile_page, @"<title>SA-MP Forums - View Profile: (.*)</title>");

            if (match.Success)
                return match.Groups[0].Value.Remove(0, 36).Replace("</title>", "");

            return string.Empty;
        }
    }
}
