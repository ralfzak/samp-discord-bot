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
    class ServerAdPurgeService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private System.Threading.Timer PurgeTimer;

        public ServerAdPurgeService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.MessageReceived += OnMessageReceivedAsync;

            PurgeTimer = new System.Threading.Timer(this.OnPurgeCheck, null, 53000, 605000);
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

            LoggerService.Write($"Storing message id {message.Id} sent by {message.Author.Username}#{message.Author.Discriminator} to Server Adv Channel.");
            StoreAdvertisementMessage(message.Id, message.Author.Id);

            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        private async void OnPurgeCheck(object state)
        {
            try
            {
                var msgs = new List<ulong>();
                var users = new List<ulong>();
                GetAdvertisementMessagesToPurge(out msgs, out users);
                LoggerService.Write($"[OnPurgeCheck] invoked: {msgs.Count} expired messages ready to be purged");

                if (msgs.Count > 0)
                {
                    // Since there are users, join them:
                    var mentioned_users = String.Join(", ", users.Select(u => ($"<@{u}>")).ToArray());

                    var adv_channel = _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADVERT_CHAN_ID);
                    foreach (var msgid in msgs)
                    {
                        RemoveAdvertisementMessage(msgid);
                        await adv_channel.DeleteMessageAsync(msgid);
                    }

                    await _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID).SendMessageAsync(
                        $"Purging {msgs.Count} message(s) from <#{Program.ADVERT_CHAN_ID}> by: {mentioned_users}"
                    );
                }
            }
            catch (Exception e)
            {
                LoggerService.Write(e.ToString());

                await _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID).SendMessageAsync(
                    $"Exception in OnPurgeCheck: {e.Message}");

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
