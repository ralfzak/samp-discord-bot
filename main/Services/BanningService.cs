using System.Collections.Generic;
using main.Core;
using main.Core.Domain.Models;
using main.Core.Domain.Repo;

namespace main.Services
{
    public class BanningService
    {
        private IBansRepository _bansRepository;
        private ITimeProvider _timeProvider;

        public BanningService(ITimeProvider timeProvider, IBansRepository bansRepository)
        {
            _timeProvider = timeProvider;
            _bansRepository = bansRepository;
        }
        
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

        public void RemoveBan(ulong userid) => 
            _bansRepository.DeleteByUserId(userid);

        public List<Bans> GetBans(string search) => 
            _bansRepository.GetBans(search);

        public List<Bans> GetExpiredBans() => 
            _bansRepository.GetExpiredBans();
    }
}
