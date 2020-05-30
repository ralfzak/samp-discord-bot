using System;
using System.Collections.Generic;
using System.Linq;
using main.Core;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace main.Services
{
    public enum VERIFICATION_STATES
    {
        NONE = 0,
        WAITING_CONFIRM = 1
    }

    public class VerificationService
    {
        private DatabaseContext _databaseContext;
        
        public VerificationService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public List<ulong> GetUserIDsFromForumInfo(string forumInfo)
        {
            int forumId = -1;
            string forumName = "-";
            if (!Int32.TryParse(forumInfo, out forumId)) {
                forumName = forumInfo;
            }

            return _databaseContext.Verifications
                .Where(v => v.ForumId == forumId || v.ForumName == forumName)
                .Select(v => v.Userid)
                .ToList();
        }

        public void GetUserForumProfileID(ulong userId, out int forumId, out string forumName)
        {
            var verification = _databaseContext.Verifications
                .FirstOrDefault(v => v.Userid == userId);
            
            forumId = -1;
            forumName = string.Empty;
            if (verification != null)
            {
                forumId = verification.ForumId ?? -1;
                forumName = verification.ForumName;
            }
        }

        public bool IsForumProfileLinked(int forumId)
        {
            return _databaseContext.Verifications
                .FirstOrDefault(v => v.ForumId == forumId) != null;
        }

        public bool IsUserVerified(ulong userId)
        {
            GetUserForumProfileID(userId, out var forumId, out var forumName);
            return (forumId != -1);
        }

        public void StoreUserVerification(ulong userId, int forumId, string forumName, string verifiedBy)
        {
            _databaseContext.Verifications.Add(new Verifications()
            {
                Userid = userId,
                ForumId = forumId,
                ForumName = forumName,
                VerifiedBy = verifiedBy
            });
            
            _databaseContext.SaveChangesAsync();
        }

        public void DeleteUserVerification(ulong userId)
        {
            var verification = _databaseContext.Verifications
                .FirstOrDefault(v => v.Userid == userId);
            
            if (verification != null)
                _databaseContext.Verifications.Remove(verification);
            
            _databaseContext.SaveChangesAsync();
        }
        
        public async Task<string> GetForumProfileContentAsync(int profileID)
        {
            string url = $"{Program.FORUM_PROFILE_URL}{profileID}";
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
