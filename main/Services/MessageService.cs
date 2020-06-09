using domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace main.Services
{
    public class MessageService
    {
        private ITimeProvider _timeProvider;
        private readonly Dictionary<ulong, MessageData> entryMap;

        public MessageService(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            entryMap = new Dictionary<ulong, MessageData>();
        }

        public void LogCommand(ulong commandId, ulong responseId)
        {
            var data = new MessageData()
            {
                responseId = responseId,
                sentOn = _timeProvider.UtcNow
            };

            entryMap.Add(commandId, data);
        }

        public ulong GetResponseFromCommandLogEntry(ulong commandId)
        {
            if (entryMap.ContainsKey(commandId))
            {
                var entry = entryMap[commandId];
                entryMap.Remove(commandId);

                return entry.responseId;
            }

            return 0;
        }

        public void DropCommandLogEntry()
        {
            var expired = new List<ulong>();

            foreach (var kvp in entryMap)
            {
                if (kvp.Value.sentOn < (_timeProvider.UtcNow.AddHours(-5)))
                {
                    expired.Add(kvp.Key);
                }
            }

            expired.ForEach(id => entryMap.Remove(id));
        }

        private class MessageData
        {
            public ulong responseId;
            public DateTime sentOn;
        }
    }
}
