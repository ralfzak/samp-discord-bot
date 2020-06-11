using main.Core;
using System;
using System.Collections.Generic;

namespace main.Services
{
    public class MessageService
    {
        private ITimeProvider _timeProvider;
        private readonly Dictionary<ulong, MessageData> _entryMap;

        public MessageService(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _entryMap = new Dictionary<ulong, MessageData>();
        }

        public void LogCommand(ulong commandId, ulong responseId)
        {
            var data = new MessageData()
            {
                ResponseId = responseId,
                SentOn = _timeProvider.UtcNow
            };

            _entryMap.Add(commandId, data);
        }

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
