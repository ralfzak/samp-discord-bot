using System;
using System.Collections.Generic;
using System.Linq;

namespace app.Services
{
    public class UserService
    {
        private Dictionary<string, long> _cooldownMap;

        public UserService()
        {
            _cooldownMap = new Dictionary<string, long>();
        }

        private string GetMapKey(ulong userId, string channel)
        {
            return $"{userId}-{channel}";
        }

        private bool IsOnCooldown(long ticks)
        {
            return (new TimeSpan(ticks).Subtract(new TimeSpan(DateTime.UtcNow.Ticks)).Ticks > 0) || false;
        }

        public void SetUserCooldown(ulong userId, string cmd, int seconds)
        {
            string key = GetMapKey(userId, cmd);
            long ticksUTC = (DateTime.UtcNow.AddSeconds(seconds).Ticks);

            if (_cooldownMap.ContainsKey(key))
            {
                _cooldownMap[key] = ticksUTC;
            }
            else _cooldownMap.Add(key, ticksUTC);
        }

        public bool IsUserOnCooldown(ulong userId, string cmd)
        {
            string key = GetMapKey(userId, cmd);

            // remove all keys with low cooldown
            if (_cooldownMap.Count > 0)
            {
                var expiredKeys = _cooldownMap
                    .Where(kp => (IsOnCooldown(kp.Value) && kp.Key != key))
                    .Select(kp => kp.Key)
                    .ToList();

                foreach (var k in expiredKeys)
                {
                    _cooldownMap.Remove(k);
                }
            }
            
            if (_cooldownMap.ContainsKey(key))
            {
                return IsOnCooldown(_cooldownMap[key]) || false;
            }

            return false;
        }

        public long[] GetUserIDsFromForumInfo(string forumInfo)
        {
            List<long> user_ids = new List<long>();

            int fid = -1;
            string fname = "-";
            if (!Int32.TryParse(forumInfo, out fid)) {
                fname = forumInfo;
            }

            var data = DataService.Get("SELECT `userid` FROM `verifications` WHERE `forumid`=@fid OR `forum_name`=@fname LIMIT 1;", new Dictionary<string, object>()
            {
                {"@fid", fid},
                {"@fname", fname}
            });

            if (data.Count > 0)
            {
                foreach (var uid in data["userid"])
                {
                    user_ids.Add((long)uid);
                }
            }

            return user_ids.ToArray();
        }

        public void GetUserForumProfileID(ulong userId, out int fid, out string fname)
        {
            fid = -1;
            fname = string.Empty;

            var data = DataService.Get("SELECT `forumid`, `forum_name` FROM `verifications` WHERE `userid`=@uid LIMIT 1;", new Dictionary<string, object>()
            {
                {"@uid", userId}
            });

            if (data.Count > 0)
            {
                fid = (int)data["forumid"][0];
                fname = (string)data["forum_name"][0];
            }
        }

        public bool IsForumProfileLinked(int profileId)
        {
            ulong userid = 0;

            var data = DataService.Get("SELECT `userid` FROM `verifications` WHERE `forumid`=@fid LIMIT 1;", new Dictionary<string, object>()
            {
                {"@fid", profileId}
            });

            if (data.Count > 0)
            {
                userid = (ulong)(long)data["userid"][0];
            }

            return (userid != 0) || false;
        }

        public bool IsUserVerified(ulong userId)
        {
            int fid = -1;
            string fname = string.Empty;
;
            GetUserForumProfileID(userId, out fid, out fname);
            return (fid != -1);
        }

        public void StoreUserVerification(ulong uid, int fid, string forumName, string discordUser)
        {
            DataService.Put("INSERT INTO verifications (`forumid`, `userid`, `forum_name`, `by`) VALUES (@fid, @uid, @fname, @by)", new Dictionary<string, object>()
            {
                {"@fid", fid},
                {"@uid", uid},
                {"@fname", forumName},
                {"@by", discordUser}
            });
        }

        public void DeleteUserVerification(ulong uid)
        {
            DataService.Put("DELETE FROM verifications WHERE `userid`=@uid;", new Dictionary<string, object>()
            {
                {"@uid", uid}
            });
        }
    }
}
