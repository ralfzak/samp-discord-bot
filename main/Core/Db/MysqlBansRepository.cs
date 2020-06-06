using System;
using System.Collections.Generic;
using domain.Models;
using domain.Repo;
using System.Linq;

namespace main.Core.Db
{
    /**
     * Persists [Bans] instances using [DatabaseContext].
     */
    public partial class MysqlBansRepository: IBansRepository
    {
        private readonly DatabaseContext _databaseContext;
        private ITimeProvider _timeProvider;
        
        public MysqlBansRepository(DatabaseContext databaseContext, ITimeProvider timeProvider)
        {
            _databaseContext = databaseContext;
            _timeProvider = timeProvider;
        }
        
        public void Create(Bans ban)
        {
            _databaseContext.Bans.Add(ban);
            _databaseContext.SaveChangesAsync();
            Logger.Write($"[Create - Bans] {ban.Userid} {ban.ByUserid}");
        }

        public void DeleteByUserId(ulong userId)
        { 
            var ban = _databaseContext.Bans.FirstOrDefault(b => b.Userid == userId);
            if (ban == null)
                return;
            
            ban.ExpiresOn = _timeProvider.UtcNow;
            _databaseContext.Bans.Update(ban);
            _databaseContext.SaveChangesAsync();
            Logger.Write($"[DeleteByUserId - Bans] {userId}");
        }

        public List<Bans> GetBans(string criteria) =>
            (UInt64.TryParse(criteria, out ulong searchId)) 
                ? _databaseContext.Bans.Where(b => b.Userid == searchId && b.ExpiresOn == null).ToList() 
                : _databaseContext.Bans.Where(b => b.Name.Contains(criteria) && b.ExpiresOn == null).ToList();
        
        public List<Bans> GetExpiredBans() => 
            _databaseContext.Bans.Where(b => b.ExpiresOn != null && b.ExpiresOn < _timeProvider.UtcNow)
                .ToList();
    }
}