using System;
using System.Collections.Generic;
using System.Linq;
using domain.Models;
using main.Core;
using main.Models;

namespace main.Services
{
    public class BanningService
    {
        private DatabaseContext _databaseContext;

        public BanningService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public void StoreBan(ulong uid, string name, ulong byuid, string byname, int secondsadd, string reason)
        {
            var ban = new Bans()
            {
                Userid = uid,
                Name = name,
                ByUserid = byuid,
                ByName = byname,
                ExpiresOn = DateTime.UtcNow.AddSeconds(secondsadd),
                Reason = reason
            };

            if (secondsadd == 0)
            {
                ban.ExpiresOn = null; // because c# is a bitch when it comes to operators
            }
            
            _databaseContext.Bans.Add(ban);

            Logger.Write($"[StoreBan] {uid} {name} {byuid} {byname} {secondsadd} {reason}");
        }

        public void RemoveBan(ulong userid)
        {
            var ban = _databaseContext.Bans
                .FirstOrDefault(b => b.Userid == userid);

            if (ban != null)
                _databaseContext.Bans.Remove(ban);
            
            Logger.Write($"[RemoveBan] {userid}");
        }

        public List<Bans> GetBans(string search)
        {
            if (UInt64.TryParse(search, out ulong v))
            {
                return _databaseContext.Bans
                    .Where(b => b.Userid == v && b.IsExpired == "N")
                    .ToList();
            }
            
            return _databaseContext.Bans
                .Where(b => b.Name == search && b.IsExpired == "N")
                .ToList();
        }

        public List<Bans> GetExpiredBans()
        {
            return _databaseContext.Bans
                .Where(b => b.ExpiresOn != null && b.IsExpired == "N" && b.ExpiresOn < DateTime.UtcNow)
                .ToList();
        }
    }
}
