using app.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace app.Services
{
    public class MessageService
    {
        public void LogCommand(ulong commandId, ulong responseId, ulong userId)
        {
            DataService.Put("INSERT INTO `command_log` (`command_id`, `response_id`, `user_id`) VALUES (@commandId, @responseId, @userId)",
                new Dictionary<string, object>
                {
                    {"@commandId", commandId},
                    {"@responseId", responseId},
                    {"@userId", userId}
                });
        }

        public ulong GetResponseFromCommandLogEntry(ulong commandId)
        {
            var data = DataService.Get(
                $"SELECT `response_id` FROM `command_log` WHERE `command_id` = @search",
                new Dictionary<string, object>()
                {
                    {"@search", commandId}
                });

            return (data.Count > 0) ? (ulong)(long)data["response_id"][0] : 0;
        }

        public void DropCommandLogEntry()
        {
            DataService.Drop("DROP FROM `command_log` WHERE `on` < FROM_UNIXTIME(UNIX_TIMESTAMP(NOW()) - 18000)", null);
        }
    }
}
