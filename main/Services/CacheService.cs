using System;
using System.Collections.Generic;
using main.Core.Domain;

namespace main.Services
{
    /// <summary>
    /// Handles all in-memory data caching.
    /// </summary>
    public class CacheService
    {
        private readonly Dictionary<string, string> _cacheMap;

        public CacheService()
        {
            _cacheMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Fetches a cached forum Id indexed by a given <paramref name="userId"/> 
        /// </summary>
        /// <param name="userId">The user Id of the indexed cache object</param>
        /// <returns>A forum profile Id</returns>
        public int GetUserForumId(ulong userId)
        {
            string key = $"{CacheStatesPrefix.Forum}-{userId}";
            return (_cacheMap.ContainsKey(key)) ? Int32.Parse(_cacheMap[key]) : -1;
        }

        /// <summary>
        /// Caches a given <paramref name="forumId"/> indexed by a given <paramref name="userId"/> 
        /// </summary>
        /// <param name="userId">The user Id of the indexed cache object, used as a key</param>
        /// <param name="forumId">The forum Id of the indexed cache object, as the actual cache value</param>
        public void SetUserForumId(ulong userId, int forumId)
        {
            string key = $"{CacheStatesPrefix.Forum}-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = forumId.ToString();
            }
            else _cacheMap.Add(key, forumId.ToString());
        }

        /// <summary>
        /// Fetches a cached token indexed by a given <paramref name="userId"/> 
        /// </summary>
        /// <param name="userId">The user Id of the indexed cache object</param>
        /// <returns>A token string</returns>
        public string GetUserToken(ulong userId)
        {
            string key = $"{CacheStatesPrefix.Token}-{userId}";
            return (_cacheMap.ContainsKey(key)) ? _cacheMap[key] : "";
        }

        /// <summary>
        /// Caches a given <paramref name="token"/> indexed by a given <paramref name="userId"/> 
        /// </summary>
        /// <param name="userId">The user Id of the indexed cache object, used as a key</param>
        /// <param name="token">The token key of the indexed cache object, as the actual cache value</param>
        public void SetUserToken(ulong userId, string token)
        {
            string key = $"{CacheStatesPrefix.Token}-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = token;
            }
            else _cacheMap.Add(key, token);
        }

        /// <summary>
        /// Fetches a cached <see cref="VerificationStates"/> indexed by a given <paramref name="userId"/> 
        /// </summary>
        /// <param name="userId">The user Id of the indexed cache object</param>
        /// <returns>A <see cref="VerificationStates"/> value</returns>
        public VerificationStates GetUserVerificationState(ulong userId)
        {
            string key = $"{CacheStatesPrefix.State}-{userId}";
            return (_cacheMap.ContainsKey(key)) 
                ? ((VerificationStates)Int32.Parse(_cacheMap[key])) 
                : VerificationStates.None;
        }

        /// <summary>
        /// Caches a given <paramref name="state"/> indexed by a given <paramref name="userId"/> 
        /// </summary>
        /// <param name="userId">The user Id of the indexed cache object, used as a key</param>
        /// <param name="state">The <see cref="VerificationStates"/> of the indexed cache object, as the actual cache value</param>
        public void SetUserVerificationState(ulong userId, VerificationStates state)
        {
            string key = $"{CacheStatesPrefix.State}-{userId}";
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = ((int)state).ToString();
            }
            else _cacheMap.Add(key, ((int)state).ToString());
        }

        /// <summary>
        /// Empties all the memory cache of a given <paramref name="userId"/>
        /// </summary>
        /// <param name="userId">The key to clear the cache on</param>
        public void ClearCache(ulong userId)
        {
            string[] keys =
            {
                $"{CacheStatesPrefix.State}-{userId}", 
                $"{CacheStatesPrefix.Forum}-{userId}", 
                $"{CacheStatesPrefix.Token}-{userId}"
            };

            foreach (string key in keys)
            {
                if (_cacheMap.ContainsKey(key))
                {
                    _cacheMap.Remove(key);
                }
            }
        }
        
        private enum CacheStatesPrefix
        {
            State = 0,
            Forum = 1,
            Token = 2
        }
    }
}
