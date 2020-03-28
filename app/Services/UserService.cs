using System;
using System.Collections.Generic;
using System.Linq;

namespace app.Services
{
    public static class UserService
    {
        private static Dictionary<string, long> COOLDOWN_MAP = new Dictionary<string, long>();

        private static string GetMapKey(ulong user_id, string channel)
        {
            return $"{user_id}-{channel}";
        }

        private static bool IsOnCooldown(long ticks)
        {
            return (new TimeSpan(ticks).Subtract(new TimeSpan(DateTime.UtcNow.Ticks)).Ticks > 0) || false;
        }

        public static void SetUserCooldown(ulong user_id, string cmd, int seconds)
        {
            string key = GetMapKey(user_id, cmd);
            long ticksUTC = (DateTime.UtcNow.AddSeconds(seconds).Ticks);

            if (COOLDOWN_MAP.ContainsKey(key))
            {
                COOLDOWN_MAP[key] = ticksUTC;
            }
            else COOLDOWN_MAP.Add(key, ticksUTC);
        }

        public static bool IsUserOnCooldown(ulong user_id, string cmd)
        {
            string key = GetMapKey(user_id, cmd);

            // remove all keys with low cooldown
            if (COOLDOWN_MAP.Count > 0)
            {
                var expiredKeys = COOLDOWN_MAP.Where(kp => (IsOnCooldown(kp.Value) && kp.Key != key)).Select(kp => kp.Key).ToList();
                foreach (var k in expiredKeys)
                {
                    COOLDOWN_MAP.Remove(k);
                }
            }

            // check if command is on cooldown
            if (COOLDOWN_MAP.ContainsKey(key))
            {
                return IsOnCooldown(COOLDOWN_MAP[key]) || false;
            }

            return false;
        }

        public static long[] GetUserIDsFromForumInfo(string forum_info)
        {
            List<long> user_ids = new List<long>();

            int fid = -1;
            string fname = "-";
            if (!Int32.TryParse(forum_info, out fid)) {
                fname = forum_info;
            }

            var data = DataService.Get("SELECT `userid` FROM `verifications` WHERE `forumid`=@fid OR `forum_name`=@fname LIMIT 1;",
                new Dictionary<string, object>()
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

        public static void GetUserForumProfileID(ulong user_id, out int fid, out string fname)
        {
            fid = -1;
            fname = string.Empty;

            var data = DataService.Get("SELECT `forumid`, `forum_name` FROM `verifications` WHERE `userid`=@uid LIMIT 1;", 
                new Dictionary<string, object>()
                {
                    {"@uid", user_id}
                });

            if (data.Count > 0)
            {
                fid = (int)data["forumid"][0];
                fname = (string)data["forum_name"][0];
            }
        }

        public static bool IsForumProfileLinked(int profile_id)
        {
            ulong userid = 0;

            var data = DataService.Get("SELECT `userid` FROM `verifications` WHERE `forumid`=@fid LIMIT 1;",
                new Dictionary<string, object>()
                {
                    {"@fid", profile_id}
                });

            if (data.Count > 0)
            {
                userid = (ulong)(long)data["userid"][0];
            }

            return (userid != 0) || false;
        }

        public static bool IsUserVerified(ulong user_id)
        {
            int fid = -1;
            string fname = string.Empty;
;
            GetUserForumProfileID(user_id, out fid, out fname);
            return (fid != -1);
        }

        public static void StoreUserVerification(ulong uid, int fid, string forum_name, string discord_user)
        {
            DataService.Put("INSERT INTO verifications (`forumid`, `userid`, `forum_name`, `by`) VALUES (@fid, @uid, @fname, @by)",
                new Dictionary<string, object>()
                {
                    {"@fid", fid},
                    {"@uid", uid},
                    {"@fname", forum_name},
                    {"@by", discord_user}
                });
        }

        public static void DeleteUserVerification(ulong uid)
        {
            DataService.Put("DELETE FROM verifications WHERE `userid`=@uid;",
                new Dictionary<string, object>()
                {
                    {"@uid", uid}
                });
        }
    }
}
