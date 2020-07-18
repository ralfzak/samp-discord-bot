using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using main.Core;
using main.Core.Domain.Repo;
using main.Core.Domain.Models;

namespace main.Services
{
    /// <summary>
    /// Encapsulates all verification related methods.
    /// </summary>
    public class VerificationService
    {
        private IVerificationsRepository _verificationsRepository;
        private IHttpClient _httpClient;
        private readonly string _forumProfileUrl;
        
        public VerificationService(IVerificationsRepository verificationsRepository, IHttpClient httpClient)
        {
            _verificationsRepository = verificationsRepository;
            _httpClient = httpClient;
            _forumProfileUrl = Configuration.GetVariable(ConfigurationKeys.UrlForumProfile);
        }

        /// <summary>
        /// Fetches a list of persisted user Ids matching a given <paramref name="forumInfo"/>.
        /// </summary>
        /// <param name="forumInfo">Forum data to be searched by: Forum name or forum Id</param>
        /// <returns>
        /// A list of verified user Ids, or an empty list if no matching instances were found.
        /// </returns>
        public List<ulong> GetUserIDsFromForumInfo(string forumInfo) =>
            _verificationsRepository.FindByForumInfo(forumInfo).Select(v => v.Userid).ToList();

        /// <summary>
        /// Fetches forum profile data of a verified given <paramref name="userId"/>
        /// </summary>
        /// <param name="userId">A verified user Id to be searched on</param>
        /// <param name="forumId">The stored forum Id of a verified user</param>
        /// <param name="forumName">The forum name of a verified user</param>
        public void GetUserForumProfileData(ulong userId, out int forumId, out string forumName)
        { 
            forumId = -1;
            forumName = string.Empty;
            
            var verification = _verificationsRepository.FindByUserId(userId);
            if (verification != null)
            {
                var liveName = GetForumProfileName(forumId);
                forumId = verification.ForumId ?? -1;
                forumName = liveName == String.Empty ? verification.ForumName : liveName;
            }
        }

        /// <summary>
        /// Checks whether a given <paramref name="forumId"/> is associated with any verification.
        /// </summary>
        /// <param name="forumId">The forum Id to be searched on</param>
        /// <returns>True if a verification is found, false otherwise</returns>
        public bool IsForumProfileLinked(int forumId) => 
            _verificationsRepository.FindByForumId(forumId) != null;

        /// <summary>
        /// Checks whether a given <paramref name="userId"/> is associated with any verification.
        /// </summary>
        /// <param name="userId">The user Id to be searched on</param>
        /// <returns>True if a verification is found, false otherwise</returns>
        public bool IsUserVerified(ulong userId) =>
            _verificationsRepository.FindByUserId(userId) != null;

        /// <summary>
        /// Persists a verification with the given parameters.
        /// </summary>
        /// <param name="userId">The verified user Id</param>
        /// <param name="forumId">The forum Id of the verified user</param>
        /// <param name="forumName">The forum name of the verified user</param>
        /// <param name="verifiedBy">The user who stored this verification</param>
        public void StoreUserVerification(ulong userId, int forumId, string forumName, string verifiedBy) =>
            _verificationsRepository.Create(new Verifications
            {
                Userid = userId,
                ForumId = forumId,
                ForumName = forumName,
                VerifiedBy = verifiedBy
            });

        /// <summary>
        /// Deletes a persisted verification instance by <paramref name="userId"/>.
        /// </summary>
        /// <remarks>
        /// If a verification is not found, the deletion will silently not occur.
        /// </remarks>
        /// <param name="userId">A user Id field of a persisted verification</param>
        public void DeleteUserVerification(ulong userId) =>
            _verificationsRepository.DeleteByUserId(userId);

        /// <summary>
        /// Fetches a forum profile name of a <paramref name="profileId"/> if the profile contains <paramref name="token"/>
        /// or an empty string otherwise.
        /// </summary>
        /// <param name="profileId">The forum profile Id</param>
        /// <param name="token">The token to be asserted on</param>
        /// <returns>The forum name if the token is found, empty string otherwise</returns>
        public string GetForumProfileNameIfContainsToken(int profileId, string token)
        {
            string profilePage = GetForumProfileContentAsync(profileId);
            return profilePage.Contains(token)
                ? GetForumProfileNameFromContent(profilePage)
                : string.Empty;
        }

        /// <summary>
        /// Fetches the forum name of a provided forum <paramref name="profileId"/>
        /// </summary>
        /// <param name="profileId">The forum profile Id</param>
        /// <returns>The forum name if exists, or an empty string otherwise</returns>
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
