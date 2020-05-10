using System;
using System.Collections.Generic;

namespace main.Services
{
    public class CacheService
    {
        private readonly Dictionary<string, string> _cacheMap;

        public CacheService()
        {
            _cacheMap = new Dictionary<string, string>();
        }

        public int GetUserForumId(ulong user_id)
        {
            string key = $"FORUMID-{user_id}";
            return (_cacheMap.ContainsKey(key)) ? Int32.Parse(_cacheMap[key]) : -1;
        }

        public void SetUserForumId(ulong user_id, int id)
        {
            string key = $"FORUMID-{user_id}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = id.ToString();
            }
            else _cacheMap.Add(key, id.ToString());
        }

        public string GetUserToken(ulong user_id)
        {
            string key = $"TOKEN-{user_id}";
            return (_cacheMap.ContainsKey(key)) ? _cacheMap[key] : "";
        }

        public void SetUserToken(ulong user_id, string token)
        {
            string key = $"TOKEN-{user_id}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = token;
            }
            else _cacheMap.Add(key, token);
        }

        public VERIFICATION_STATES GetUserVerificationState(ulong user_id)
        {
            string key = $"STATE-{user_id}";
            return (_cacheMap.ContainsKey(key)) ? ((VERIFICATION_STATES)Int32.Parse(_cacheMap[key])) : VERIFICATION_STATES.NONE;
        }

        public void SetUserVerificationState(ulong userId, VERIFICATION_STATES state)
        {
            string key = $"STATE-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = ((int)state).ToString();
            }
            else _cacheMap.Add(key, ((int)state).ToString());
        }

        public void ClearCache(ulong user_id)
        {
            string[] keys = { $"STATE-{user_id}", $"FORUMID-{user_id}", $"TOKEN-{user_id}" };

            foreach (string key in keys)
            {
                if (_cacheMap.ContainsKey(key))
                {
                    _cacheMap.Remove(key);
                }
            }
        }
    }
}
