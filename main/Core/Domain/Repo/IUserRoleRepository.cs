using System.Collections.Generic;
using main.Core.Domain.Models;

namespace main.Core.Domain.Repo
{
    /// <summary>
    /// Responsible for persisting <see cref="UserRole"/> instances.
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Persists a UserRole instance.
        /// If a role assignment already existed, the assignment will be silently ignored.
        /// </summary>
        /// <param name="userId">The Id of the user being assigned a role</param>
        /// <param name="roleId">The role Id being assigned</param>
        /// <param name="assignedBy">The Id of the user assigning the role</param>
        void Create(ulong userId, ulong roleId, ulong assignedBy);

        /// <summary>
        /// Deletes a persisted role assignment instance by <paramref name="userId"/> and <paramref name="roleId"/>.
        /// </summary>
        /// <remarks>
        /// If a role assignment is not found, the deletion will silently not occur.
        /// </remarks>
        /// <param name="userId">A user Id field of a <see cref="UserRole"/> object</param>
        /// <param name="roleId">A role Id Id field of a <see cref="UserRole"/> object</param>
        void Delete(ulong userId, ulong roleId);

        /// <summary>
        /// Fetches a list of persisted <see cref="UserRole"/> matching a given <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">A user Id field of a <see cref="UserRole"/> object</param>
        /// <returns>
        /// List of role assignment instances belonging to a given <paramref name="userId"/>, or an empty list
        /// if no records were found.
        /// </returns>
        List<UserRole> GetByUserId(ulong userId);
    }
}
