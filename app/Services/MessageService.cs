using app.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace app.Services
{
    public class MessageService
    {
        private readonly Dictionary<ulong, MessageData> entryMap;
        
        public MessageService()
        {
            entryMap = new Dictionary<ulong, MessageData>();
        }

        public void LogCommand(ulong commandId, ulong responseId)
        {
            var data = new MessageData()
            {
                responseId = responseId,
                sentOn = DateTime.Now
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
                if (kvp.Value.sentOn < (DateTime.Now.AddHours(-5)))
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
