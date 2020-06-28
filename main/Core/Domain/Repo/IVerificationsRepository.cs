using System.Collections.Generic;
using main.Core.Domain.Models;

namespace main.Core.Domain.Repo
{
    /// <summary>
    /// Responsible for persisting <see cref="Verifications"/> instances.
    /// </summary>
    public interface IVerificationsRepository
    {
        /// <summary>
        /// Persists a <see cref="Verifications"/> instance.
        /// </summary>
        /// <param name="verification">A single Verifications object with data to be persisted</param>
        void Create(Verifications verification);
        
        /// <summary>
        /// Deletes a persisted verification instance by <paramref name="userId"/>.
        /// </summary>
        /// <remarks>
        /// If a verification is not found, the deletion will silently not occur.
        /// </remarks>
        /// <param name="userId">A user Id field of a <see cref="Verifications"/> object</param>
        void DeleteByUserId(ulong userId);

        /// <summary>
        /// Fetches a list of persisted <see cref="Verifications"/> matching a given <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">Data to be searched by</param>
        /// <returns>
        /// A list of <see cref="Verifications"/>, or an empty list if no matching instances were found.
        /// </returns>
        List<Verifications> FindByForumInfo(string criteria);
        
        /// <summary>
        /// Fetches a persisted verification instance by <paramref name="forumId"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Verifications"/> object, or null if no matching record was found.
        /// </returns>
        /// <param name="forumId">A forum Id field of a <see cref="Verifications"/> object</param>
        Verifications FindByForumId(int forumId);        
        
        /// <summary>
        /// Fetches a persisted verification instance by <paramref name="userId"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Verifications"/> object, or null if no matching record was found.
        /// </returns>
        /// <param name="userId">A user Id field of a <see cref="Verifications"/> object</param>
        Verifications FindByUserId(ulong userId);
    }
}
