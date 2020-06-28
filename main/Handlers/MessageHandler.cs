using main.Services;
using Discord;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;

namespace main.Handlers
{
    /// <summary>
    /// Encapsulates message related events.
    /// Handles message monitoring for command message deletion. 
    /// </summary>
    #pragma warning disable 4014, 1998
    class MessageHandler
    {
        private readonly MessageService _messageService;
        private Timer _pruneTimer;

        public MessageHandler(DiscordSocketClient discord, MessageService messageService)
        {
            _messageService = messageService;
            _pruneTimer = new Timer(OnCommandLogPrune, null, 10000, 18000000);

            discord.MessageDeleted += OnMessageDelete;
        }

        private void OnCommandLogPrune(object state)
        {
            _messageService.DropCommandLogEntry();
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        private async Task OnMessageDelete(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel channel)
        {
            var messageId = _messageService.GetResponseFromCommandLogEntry(cacheable.Id);
            if (messageId != 0)
            {
                channel.DeleteMessageAsync(messageId);
            }
        }
    }
}
