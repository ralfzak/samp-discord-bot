using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using domain.Models;
using domain.Repo;
using main.Core;

namespace main.Services
{
    public class VerificationService
    {
        private IVerificationsRepository _verificationsRepository;
        private IHttpClient _httpClient;
        private readonly string _forumProfileUrl;
        
        public VerificationService(IVerificationsRepository verificationsRepository, IHttpClient httpClient)
        {
            _verificationsRepository = verificationsRepository;
            _httpClient = httpClient;
            _forumProfileUrl = Configuration.GetVariable("Urls.Forum.Profile");
        }

        public List<ulong> GetUserIDsFromForumInfo(string forumInfo) =>
            _verificationsRepository.FindByForumInfo(forumInfo).Select(v => v.Userid).ToList();

        public void GetUserForumProfileId(ulong userId, out int forumId, out string forumName)
        { 
            forumId = -1;
            forumName = string.Empty;
            
            var verification = _verificationsRepository.FindByUserId(userId);
            if (verification != null)
            {
                forumId = verification.ForumId ?? -1;
                forumName = verification.ForumName;
            }
        }

        public bool IsForumProfileLinked(int forumId) => 
            _verificationsRepository.FindByForumId(forumId) != null;

        public bool IsUserVerified(ulong userId) =>
            _verificationsRepository.FindByUserId(userId) != null;

        public void StoreUserVerification(ulong userId, int forumId, string forumName, string verifiedBy) =>
            _verificationsRepository.Create(new Verifications
            {
                Userid = userId,
                ForumId = forumId,
                ForumName = forumName,
                VerifiedBy = verifiedBy
            });

        public void DeleteUserVerification(ulong userId) =>
            _verificationsRepository.DeleteByUserId(userId);

        public string GetForumProfileNameIfContainsToken(int profileId, string token)
        {
            string profilePage = GetForumProfileContentAsync(profileId);
            return profilePage.Contains(token)
                ? GetForumProfileNameFromContent(profilePage)
                : string.Empty;
        }

        public string GetForumProfileName(int profileId) =>
            GetForumProfileNameFromContent(GetForumProfileContentAsync(profileId));
        
        private string GetForumProfileContentAsync(int profileId) => 
            _httpClient.GetContent($"{_forumProfileUrl}{profileId}");
        
        private string GetForumProfileNameFromContent(string content)
        {
            Match match = Regex.Match(content, @"<title>SA-MP Forums - View Profile: (.*)</title>");
            return match.Success
                ? match.Groups[0].Value.Remove(0, 36).Replace("</title>", "")
                : string.Empty;
        }
    }
}
