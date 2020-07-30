using System;
using System.Collections.Generic;
using main.Core.Domain.Models;
using main.Core.Domain.Repo;
using System.Linq;

namespace main.Core.Database
{    
    /// <summary>
    /// Persists <see cref="UserRole"/> instances using <see cref="DatabaseContext"/>.
    /// </summary>
    public class MysqlUserRoleRepository: IUserRoleRepository
    {
        private readonly DatabaseContext _databaseContext;
        private ITimeProvider _timeProvider;
        
        public MysqlUserRoleRepository(DatabaseContext databaseContext, ITimeProvider timeProvider)
        {
            _databaseContext = databaseContext;
            _timeProvider = timeProvider;
        }
        
        public void Create(ulong userId, ulong roleId, ulong assignedBy)
        {
            try
            {
                _databaseContext.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedBy = assignedBy,
                    AssignedOn = _timeProvider.UtcNow
                });
            
                _databaseContext.SaveChanges();
            }
            catch (InvalidOperationException e)
            {
                Logger.Write($"Failed role assignment {roleId} to {userId} by {assignedBy}: {e.Message}");
            }
        }

        public void Delete(ulong userId, ulong roleId)
        {
            var roleAssignment = _databaseContext.UserRoles
                .FirstOrDefault(r => r.UserId == userId && r.RoleId == roleId);
            if (roleAssignment != null)
            {
                _databaseContext.UserRoles.Remove(roleAssignment);
                _databaseContext.SaveChanges();
            }
        }

        public List<UserRole> GetByUserId(ulong userId) => 
            _databaseContext.UserRoles.Where(v => v.UserId == userId).ToList();
    }
}