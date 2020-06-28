using System.Collections.Generic;
using main.Core;
using main.Core.Domain.Models;
using main.Core.Domain.Repo;

namespace main.Services
{
    /// <summary>
    /// Contains all methods for maintaining and managing bans.
    /// </summary>
    public class BanningService
    {
        private IBansRepository _bansRepository;
        private ITimeProvider _timeProvider;

        public BanningService(ITimeProvider timeProvider, IBansRepository bansRepository)
        {
            _timeProvider = timeProvider;
            _bansRepository = bansRepository;
        }
        
        /// <summary>
        /// Persists a ban using the given parameters.
        /// </summary>
        /// <param name="uid">The banned user Id</param>
        /// <param name="name">The banned user name</param>
        /// <param name="byuid">The banning user Id</param>
        /// <param name="byname">The banning user name</param>
        /// <param name="secondsadd">Number of seconds since ban time until the ban expires</param>
        /// <param name="reason">The ban reason</param>
        public void StoreBan(ulong uid, string name, ulong byuid, string byname, int secondsadd, string reason)
        {
            var ban = new Bans
            {
                Userid = uid,
                Name = name,
                ByUserid = byuid,
                ByName = byname,
                ExpiresOn = _timeProvider.UtcNow.AddSeconds(secondsadd),
                Reason = reason
            };

            if (secondsadd == 0)
            {
                ban.ExpiresOn = null; // because c# is was not happy
            }
            
            _bansRepository.Create(ban);
        }

        /// <summary>
        /// Removes a persisted ban instance by <paramref name="userId"/>.
        /// </summary>
        /// <remarks>
        /// If a ban is not found, the removal will silently not occur.
        /// </remarks>
        /// <param name="userId">A user Id of a persisted ban object to be removed</param>
        public void RemoveBan(ulong userId) => 
            _bansRepository.DeleteByUserId(userId);

        /// <summary>
        /// Fetches a list of persisted <see cref="Bans"/> matching a given <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">Data to be searched by</param>
        /// <returns>
        /// A list of <see cref="Bans"/>, or an empty list if no matching instances were found.
        /// </returns>
        public List<Bans> GetBans(string criteria) => 
            _bansRepository.GetBans(criteria);

        /// <summary>
        /// Fetches a list of expired persisted <see cref="Bans"/>.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Bans"/>, or an empty list if no expired instances were found.
        /// </returns>
        public List<Bans> GetExpiredBans() => 
            _bansRepository.GetExpiredBans();
    }
}
