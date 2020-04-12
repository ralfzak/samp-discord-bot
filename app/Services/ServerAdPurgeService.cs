using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using app.Models;
using System.IO;

namespace app.Services
{
    #pragma warning disable 4014,1998
    class ServerAdPurgeService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private System.Threading.Timer _purgeTimer;

        public ServerAdPurgeService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.MessageReceived += OnMessageReceivedAsync;
            _discord.MessageDeleted += OnMessageDeletedAsync;

            _purgeTimer = new System.Threading.Timer(this.OnPurgeCheck, null, 53000, 605000);

            LoggerService.Write("The server ad purge check has been attached to OnPurgeCheck!");
        }

        private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            if (message.Channel.Id != Program.ADVERT_CHAN_ID)
                return;
            
            StoreAdvertisementMessage(message.Id, message.Author.Id);
        }        
        
        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel socketMessageChannel)
        {
            if (!cacheable.HasValue)
                return;
            
            var message = cacheable.Value;
            if (message.Channel.Id != Program.ADVERT_CHAN_ID)
                return;
            
            RemoveAdvertisementMessage(message.Id);
        }

        public async Task InitializeAsync()
        {
        }

        private async void OnPurgeCheck(object state)
        {
            try
            {
                GetAdvertisementMessagesToPurge(out var msgs, out var users);

                if (msgs.Count > 0)
                {
                    // Since there are users, join them:
                    var mentionedUsers = String.Join(", ", users.Select(u => ($"<@{u}>")).ToArray());
                    var advChannel = _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADVERT_CHAN_ID);
                    
                    foreach (var msgid in msgs)
                    {
                        RemoveAdvertisementMessage(msgid);
                        advChannel.DeleteMessageAsync(msgid);
                    }

                    _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID)
                        .SendMessageAsync($"Purging {msgs.Count} message(s) from <#{Program.ADVERT_CHAN_ID}> by: {mentionedUsers}");
                }
            }
            catch (Exception e)
            {
                LoggerService.Write(e.ToString());

                _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID)
                    .SendMessageAsync($"Exception in OnPurgeCheck: {e.Message}");

                if (e.Message.Contains("404"))
                {
                    RemoveAdvertisementMessage();
                }
            }
        }

        private void GetAdvertisementMessagesToPurge(out List<ulong> messages, out List<ulong> users)
        {
            messages = new List<ulong>();
            users = new List<ulong>();
            var data = DataService.Get("SELECT `mid`, `uid` FROM `advert_messages` WHERE FROM_UNIXTIME(UNIX_TIMESTAMP(`sent_on`)+21600) <= NOW()", null);

            if (data.Count > 0)
            {
                messages.AddRange(data["mid"].Select(mid => (ulong)(long)mid));
                users.AddRange(data["uid"].Select(uid => (ulong)(long)uid));
            }
        }

        private void RemoveAdvertisementMessage(ulong messageid)
        {
            DataService.Drop("DELETE FROM `advert_messages` WHERE `mid` = @msgid", new Dictionary<string, object>()
            {
                {"@msgid", messageid}
            });
        }

        private void RemoveAdvertisementMessage()
        {
            DataService.Drop("DELETE FROM `advert_messages` WHERE FROM_UNIXTIME(UNIX_TIMESTAMP(`sent_on`)+21600) <= NOW()", new Dictionary<string, object>());
        }

        private static void StoreAdvertisementMessage(ulong messageid, ulong userid)
        {
            DataService.Put($"INSERT INTO `advert_messages` (`mid`, `uid`) VALUES (@msgid, @userid)", new Dictionary<string, object>()
            {
                {"@msgid", messageid},
                {"@userid", userid}
            });
        }
    }
}
