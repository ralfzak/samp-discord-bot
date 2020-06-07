using System.Collections.Generic;
using domain.Models;

namespace domain.Repo
{
    /**
     * Responsible for persisting [Bans] instances.
     */
    public interface IBansRepository
    {
        /**
         * Persists a [Bans] instance.
         */
        void Create(Bans ban);

        /**
         * Deletes a [Bans] instance.
         */
        void DeleteByUserId(ulong userId);

        /**
         * Returns a list of [Bans] matching a given [criteria], or an empty list if no matching instances were found.
         */
        List<Bans> GetBans(string criteria);

        /**
         * Returns a list of expired [Bans], or an empty list if no matching instances were found. 
         */
        List<Bans> GetExpiredBans();
    }
}
