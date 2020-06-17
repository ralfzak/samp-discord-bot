using System.Collections.Generic;
using main.Core.Domain.Models;

namespace main.Core.Domain.Repo
{
    /// <summary>
    /// Responsible for persisting <see cref="Bans"/> instances.
    /// </summary>
    public interface IBansRepository
    {
        /// <summary>
        /// Persists a Bans instance
        /// </summary>
        /// <param name="ban">A single ban object with data to be persisted</param>
        void Create(Bans ban);

        /// <summary>
        /// Deletes a persisted ban instance by <paramref name="userId"/>.
        /// </summary>
        /// <remarks>
        /// If a ban is not found, the deletion will silently not occure.
        /// </remarks>
        /// <param name="userId">A user Id field of a <see cref="Bans"/> object</param>
        void DeleteByUserId(ulong userId);

        /// <summary>
        /// Fetches a list of persisted <see cref="Bans"/> matching a given <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">Data to be searched by</param>
        /// <returns>
        /// A list of <see cref="Bans"/>, or an empty list if no matching instances were found.
        /// </returns>
        List<Bans> GetBans(string criteria);

        /// <summary>
        /// Fetches a list of expired persisted <see cref="Bans"/>.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Bans"/>, or an empty list if no expired instances were found.
        /// </returns>
        List<Bans> GetExpiredBans();
    }
}
