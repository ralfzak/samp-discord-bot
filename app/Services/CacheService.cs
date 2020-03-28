using System;
using System.Collections.Generic;
using System.Text;

namespace app.Services
{
    public static class CacheService
    {
        private static Dictionary<string, string> CACHE_MAP = new Dictionary<string, string>();

        public static int GetUserForumID(ulong user_id)
        {
            string key = $"FORUMID-{user_id}";
            return (CACHE_MAP.ContainsKey(key)) ? Int32.Parse(CACHE_MAP[key]) : -1;
        }

        public static void SetUserForumID(ulong user_id, int id)
        {
            string key = $"FORUMID-{user_id}";
            if (CACHE_MAP.ContainsKey(key))
            {
                CACHE_MAP[key] = id.ToString();
            }
            else CACHE_MAP.Add(key, id.ToString());
        }

        public static string GetUserToken(ulong user_id)
        {
            string key = $"TOKEN-{user_id}";
            return (CACHE_MAP.ContainsKey(key)) ? CACHE_MAP[key] : "";
        }

        public static void SetUserToken(ulong user_id, string token)
        {
            string key = $"TOKEN-{user_id}";
            if (CACHE_MAP.ContainsKey(key))
            {
                CACHE_MAP[key] = token;
            }
            else CACHE_MAP.Add(key, token);
        }

        public static VERIFICATION_STATES GetUserVerificationState(ulong user_id)
        {
            string key = $"STATE-{user_id}";
            return (CACHE_MAP.ContainsKey(key)) ? ((VERIFICATION_STATES)Int32.Parse(CACHE_MAP[key])) : VERIFICATION_STATES.NONE;
        }

        public static void SetUserVerificationState(ulong user_id, VERIFICATION_STATES state)
        {
            string key = $"STATE-{user_id}";
            if (CACHE_MAP.ContainsKey(key))
            {
                CACHE_MAP[key] = ((int)state).ToString();
            }
            else CACHE_MAP.Add(key, ((int)state).ToString());
        }

        public static void ClearCache(ulong user_id)
        {
            string[] keys = { $"STATE-{user_id}", $"FORUMID-{user_id}", $"TOKEN-{user_id}" };

            foreach (string key in keys)
            {
                if (CACHE_MAP.ContainsKey(key))
                {
                    CACHE_MAP.Remove(key);
                }
            }
        }
    }
}
