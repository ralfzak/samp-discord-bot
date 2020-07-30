using System.Collections.Generic;
using System.Linq;
using main.Core;
using main.Core.Domain.Repo;

namespace main.Services
{
    /// <summary>
    /// Encapsulates all user related functions.
    /// </summary>
    public class UserService
    {
        private ITimeProvider _timeProvider;
        private Dictionary<string, long> _cooldownMap;
        private IUserRoleRepository _userRoleRepository;

        public UserService(ITimeProvider timeProvider, IUserRoleRepository userRoleRepository)
        {
            _timeProvider = timeProvider;
            _userRoleRepository = userRoleRepository;
            _cooldownMap = new Dictionary<string, long>();
        }

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
        
        /// <summary>
        /// Returns a list of role ids assigned to a specific given user id.
        /// </summary>
        /// <param name="userId">The unique Id of a user having roles assigned to</param>
        /// <returns>List of role Ids, or empty list if none exist</returns>
        public List<ulong> GetUserRolesIds(ulong userId) => 
            _userRoleRepository.GetByUserId(userId).Select(r => r.RoleId).ToList();

        /// <summary>
        /// Assigns a user a specific role 
        /// </summary>
        /// <param name="userId">The Id of the user being assigned a role</param>
        /// <param name="roleId">The role Id being assigned</param>
        /// <param name="assignedById"></param>
        public void AssignUserRole(ulong userId, ulong roleId, ulong assignedById) =>
            _userRoleRepository.Create(userId, roleId, assignedById);

        /// <summary>
        /// Deletes a persisted role assignment instance by <paramref name="userId"/>.
        /// </summary>
        /// <remarks>
        /// If a role assignment is not found, the deletion will silently not occur.
        /// </remarks>
        /// <param name="userId">A user Id field of a <see cref="UserRole"/> object</param>
        /// <param name="roleId">A role Id field of a <see cref="UserRole"/> object</param>
        public void DeleteUserRole(ulong userId, ulong roleId) =>
            _userRoleRepository.Delete(userId, roleId);
        
        private string GetMapKey(ulong userId, string channel) =>
            $"{userId}-{channel}";

        private bool IsOnCooldown(long ticks) =>
            _timeProvider.GetElapsedFromEpoch(ticks) > 0;
    }
}
