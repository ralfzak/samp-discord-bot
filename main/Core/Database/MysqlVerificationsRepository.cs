using System;
using System.Collections.Generic;
using System.Linq;
using main.Core;
using main.Core.Repo;

namespace main.Core.Database
{
    /**
     * Persists [Verifications] instances using [DatabaseContext].
     */
    public partial class MysqlVerificationsRepository : IVerificationsRepository
    {
        private readonly DatabaseContext _databaseContext;
        private ITimeProvider _timeProvider;

        public MysqlVerificationsRepository(DatabaseContext databaseContext, ITimeProvider timeProvider)
        {
            _databaseContext = databaseContext;
            _timeProvider = timeProvider;
        }

        public void Create(Verifications verification)
        {
            _databaseContext.Verifications.Add(verification);
            _databaseContext.SaveChangesAsync();
        }

        public void DeleteByUserId(ulong userId)
        {
            var verification = _databaseContext.Verifications.FirstOrDefault(v => v.Userid == userId);
            if (verification != null)
            {
                verification.DeletedOn = _timeProvider.UtcNow;
                _databaseContext.Verifications.Update(verification);
                _databaseContext.SaveChangesAsync();
            }
        }

        public List<Verifications> FindByForumInfo(string criteria)
        {
            int forumId = -1;
            Int32.TryParse(criteria, out forumId);
            return _databaseContext.Verifications
                .Where(v => v.ForumId == forumId || v.ForumName == criteria)
                .ToList();
        }

        public Verifications FindByForumId(int forumId) =>
            _databaseContext.Verifications.FirstOrDefault(v => v.ForumId == forumId);

        public Verifications FindByUserId(ulong userId) => 
            _databaseContext.Verifications.FirstOrDefault(v => v.Userid == userId);
    }
}
