using System;
using System.Collections.Generic;
using System.Linq;
using domain.Models;

namespace main.Services
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
    }
}
