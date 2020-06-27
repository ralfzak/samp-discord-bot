using System.Collections.Generic;
using System.Linq;
using main.Core;

namespace main.Services
{
    /// <summary>
    /// Encapsulates all user related functions.
    /// </summary>
    public class UserService
    {
        private ITimeProvider _timeProvider;
        private Dictionary<string, long> _cooldownMap;

        public UserService(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _cooldownMap = new Dictionary<string, long>();
        }

        private string GetMapKey(ulong userId, string channel) =>
            $"{userId}-{channel}";

        private bool IsOnCooldown(long ticks) =>
            _timeProvider.GetElapsedFromEpoch(ticks) > 0;

        /// <summary>
        /// Sets a given <paramref name="userId"/> a <paramref name="identifier"/> key with a cooldown of <paramref name="numberOfSeconds"/>
        /// </summary>
        /// <param name="userId">The user Id to be set on cooldown</param>
        /// <param name="numberOfSeconds">Number of seconds until the cooldown is over</param>
        /// <param name="identifier">A key used to differentiate different cooldowns set</param>
        public void SetUserCooldown(ulong userId, int numberOfSeconds, string identifier = "")
        {
            string key = GetMapKey(userId, identifier);
            long ticksUtc = (_timeProvider.UtcNow.AddSeconds(numberOfSeconds).Ticks);

            if (_cooldownMap.ContainsKey(key))
            {
                _cooldownMap[key] = ticksUtc;
            }
            else _cooldownMap.Add(key, ticksUtc);
        }

        /// <summary>
        /// Returns whether a given <paramref name="userId"/> is on cooldown differentiated by a given <paramref name="identifier"/>.
        /// This command also drops all expired cooldowns in order to free memory.
        /// </summary>
        /// <param name="userId">The user id used to check if on cooldown</param>
        /// <param name="identifier">The command string to differentiated different set cooldowns</param>
        /// <returns>True if a user is on cooldown, false otherwise</returns>
        public bool IsUserOnCooldown(ulong userId, string identifier = "")
        {
            string key = GetMapKey(userId, identifier);

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
    }
}
