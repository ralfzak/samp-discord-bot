using main.Core;
using System;
using System.Collections.Generic;

namespace main.Services
{
    /// <summary>
    /// Responsible for maintaining messages.
    /// </summary>
    public class MessageService
    {
        private ITimeProvider _timeProvider;
        private readonly Dictionary<ulong, MessageData> _entryMap;

        public MessageService(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _entryMap = new Dictionary<ulong, MessageData>();
        }

        /// <summary>
        /// Logs a <paramref name="commandId"/> associated with a <paramref name="responseId"/> along with a timestamp
        /// of the current time.
        /// The time is used to drop the given instance after 5 hours of adding it.
        /// </summary> 
        /// <param name="commandId">A command Id, used to search with</param>
        /// <param name="responseId">A response to a command Id, used to be fetched</param>
        public void LogCommand(ulong commandId, ulong responseId)
        {
            var data = new MessageData()
            {
                ResponseId = responseId,
                SentOn = _timeProvider.UtcNow
            };

            _entryMap.Add(commandId, data);
        }

        /// <summary>
        /// Retrieves a stored response Id associated with a given <paramref name="commandId"/>
        /// </summary>
        /// <param name="commandId">The command Id to search with</param>
        /// <returns>A response Id of a given <paramref name="commandId"/>, or 0 if no matching association was found</returns>
        public ulong GetResponseFromCommandLogEntry(ulong commandId)
        {
            if (_entryMap.ContainsKey(commandId))
            {
                var entry = _entryMap[commandId];
                _entryMap.Remove(commandId);

                return entry.ResponseId;
            }

            return 0;
        }

        /// <summary>
        /// Drops all expired stored entries.
        /// Entries are marked as expired if they have been inserted more than 5 hours of the time calling this method.
        /// </summary>
        public void DropCommandLogEntry()
        {
            var expired = new List<ulong>();

            foreach (var kvp in _entryMap)
            {
                if (kvp.Value.SentOn < (_timeProvider.UtcNow.AddHours(-5)))
                {
                    expired.Add(kvp.Key);
                }
            }

            expired.ForEach(id => _entryMap.Remove(id));
        }

        private class MessageData
        {
            public ulong ResponseId;
            public DateTime SentOn;
        }
    }
}
