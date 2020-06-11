using System.Collections.Generic;
using System.Linq;
using main.Core;

namespace main.Services
{
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

        public void SetUserCooldown(ulong userId, string cmd, int seconds)
        {
            string key = GetMapKey(userId, cmd);
            long ticksUtc = (_timeProvider.UtcNow.AddSeconds(seconds).Ticks);

            if (_cooldownMap.ContainsKey(key))
            {
                _cooldownMap[key] = ticksUtc;
            }
            else _cooldownMap.Add(key, ticksUtc);
        }

        public bool IsUserOnCooldown(ulong userId, string cmd = "")
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
    }
}
