using main.Core;
using main.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace main.Handlers
{
    #pragma warning disable 4014, 1998
    class MessageHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly MessageService _messageService;
        private readonly Timer _event;

        public MessageHandler(IServiceProvider services, MessageService messageService)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _messageService = messageService;
            _event = new Timer(this.OnCommandLogPrune, null, 10000, 18000000);

            _discord.MessageDeleted += OnMessageDelete;

            Logger.Write("The message handlers have been injected!");
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
                Logger.Write($"[OnMessageDelete] Command {cacheable.Id} deleted, deleting response {messageId}");
                channel.DeleteMessageAsync(messageId);
            }
        }
        
    }
}
