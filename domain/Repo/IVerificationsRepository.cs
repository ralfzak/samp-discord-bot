using System.Collections.Generic;
using main.Core;

namespace main.Core.Repo
{
    /**
     * Responsible for persisting [Verifications] instances.
     */
    public interface IVerificationsRepository
    {
        /**
         * Persists a [Verifications] instance.
         */
        void Create(Verifications verification);

        /**
         * Drops a [Verifications] instance.
         */
        void DeleteByUserId(ulong userId);

        /**
         * Returns a list of [Verifications] matching a given [criteria], or an empty list if no matching instances
         * were found.
         */
        List<Verifications> FindByForumInfo(string criteria);
        
        /**
         * Returns a [Verifications] matching a given [forumId], or null if no matching instance was found.
         */
        Verifications FindByForumId(int forumId);        
        
        /**
         * Returns a [Verifications] matching a given [userId], or null if no matching instance was found.
         */
        Verifications FindByUserId(ulong userId);
    }
}
