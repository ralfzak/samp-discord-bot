using System;
using System.Collections.Generic;
using main.Core;

namespace main.Services
{
    public class CacheService
    {
        private readonly Dictionary<string, string> _cacheMap;

        public CacheService()
        {
            _cacheMap = new Dictionary<string, string>();
        }

        public int GetUserForumId(ulong userId)
        {
            string key = $"FORUMID-{userId}";
            return (_cacheMap.ContainsKey(key)) ? Int32.Parse(_cacheMap[key]) : -1;
        }

        public void SetUserForumId(ulong userId, int forumId)
        {
            string key = $"FORUMID-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = forumId.ToString();
            }
            else _cacheMap.Add(key, forumId.ToString());
        }

        public string GetUserToken(ulong userId)
        {
            string key = $"TOKEN-{userId}";
            return (_cacheMap.ContainsKey(key)) ? _cacheMap[key] : "";
        }

        public void SetUserToken(ulong userId, string token)
        {
            string key = $"TOKEN-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = token;
            }
            else _cacheMap.Add(key, token);
        }

        public VerificationStates GetUserVerificationState(ulong userId)
        {
            string key = $"STATE-{userId}";
            return (_cacheMap.ContainsKey(key)) 
                ? ((VerificationStates)Int32.Parse(_cacheMap[key])) 
                : VerificationStates.None;
        }

        public void SetUserVerificationState(ulong userId, VerificationStates state)
        {
            string key = $"STATE-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = ((int)state).ToString();
            }
            else _cacheMap.Add(key, ((int)state).ToString());
        }

        public void ClearCache(ulong userId)
        {
            string[] keys = { $"STATE-{userId}", $"FORUMID-{userId}", $"TOKEN-{userId}" };

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
