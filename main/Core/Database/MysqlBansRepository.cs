using System;
using System.Collections.Generic;
using System.Linq;
using main.Core.Domain.Models;
using main.Core.Domain.Repo;

namespace main.Core.Database
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
            _databaseContext.SaveChanges();
            Logger.Write($"[Create - Bans] {ban.Userid} {ban.ByUserid}");
        }

        public void DeleteByUserId(ulong userId)
        { 
            var ban = _databaseContext.Bans.FirstOrDefault(b => b.Userid == userId);
            if (ban == null)
                return;
            
            ban.Lifted = 1;
            _databaseContext.Bans.Update(ban);
            _databaseContext.SaveChanges();
            Logger.Write($"[DeleteByUserId - Bans] {userId}");
        }

        public List<Bans> GetBans(string criteria) =>
            (UInt64.TryParse(criteria, out ulong searchId)) 
                ? _databaseContext.Bans.Where(b => b.Userid == searchId).ToList() 
                : _databaseContext.Bans.Where(b => b.Name.Contains(criteria)).ToList();
        
        public List<Bans> GetExpiredBans() => 
            _databaseContext.Bans
                .Where(b => b.ExpiresOn != null && b.ExpiresOn < _timeProvider.UtcNow && 
                            b.Lifted == 0)
                .ToList();
    }
}
