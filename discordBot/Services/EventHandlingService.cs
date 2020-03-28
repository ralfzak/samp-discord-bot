using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace discordBot.Services
{
    class EventHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public EventHandlingService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            
            _discord.UserJoined += OnUserJoinServer;
            _discord.UserLeft += OnUserLeaveServer;
            _discord.MessageDeleted += OnMessageDelete;
            _discord.MessageUpdated += OnMessageUpdate;

            LoggerService.Write("Binded the user (connect / disconnect) events!");
        }

        private async Task OnMessageUpdate(Cacheable<IMessage, ulong> oldmsg, SocketMessage newmsg, ISocketMessageChannel channel)
        {
            if (oldmsg.HasValue)
            {
                if ((oldmsg.Value.Content != newmsg.Content) && newmsg.EditedTimestamp.HasValue) // a msg was edited
                {
                    if (newmsg.Author.Id == Program.BOT_ID)
                        return;

                    if (newmsg.Channel.Id == Program.ADVERT_CHAN_ID)
                        return;

                    if (!(newmsg.Channel is ITextChannel))
                        return;

                    await _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID).SendMessageAsync(
                        $"Message by <@{newmsg.Author.Id}> edited in <#{channel.Id}>" +
                        "\n" +
                        $"From: **{StringService.Trim(oldmsg.Value.Content)}**" +
                        "\n" +
                        $"To: **{StringService.Trim(newmsg.Content)}**");
                }
            }
        }

        private async Task OnMessageDelete(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            if (msg.HasValue)
            {
                LoggerService.Write($"[OnMessageDelete] {msg.Value.Author.Discriminator} - {channel.Name}: {msg.Value.Content}");

                if (msg.Value.Content.Contains("Message by"))
                    return;

                if (msg.Value.Channel.Id == Program.ADVERT_CHAN_ID)
                    return;

                if (!(msg.Value.Channel is ITextChannel))
                    return;

                await _discord.GetGuild(Program.GUILD_ID).GetTextChannel(Program.ADMIN_CHAN_ID).SendMessageAsync(
                    $"Message by <@{msg.Value.Author.Id}> deleted in <#{channel.Id}>: **{StringService.Trim(msg.Value.Content)}**");
            }
            else
            {
                LoggerService.Write($"[OnMessageDelete] In {channel.Name} failed to fetch from cache");
            }

            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task OnUserJoinServer(SocketGuildUser user)
        {
            if (UserService.IsUserVerified(user.Id))
            {
                LoggerService.Write($"> JOIN VERIFIED: {user.Id} - ROLE SET");
                var verifiedRole = _discord.Guilds.FirstOrDefault(g => g.Id == Program.GUILD_ID).Roles.FirstOrDefault(r => r.Id == Program.VERIFIED_ROLE_ID);
                await user.AddRoleAsync(verifiedRole);
            }

            CacheService.ClearCache(user.Id);
            await Task.CompletedTask;
        }

        public async Task OnUserLeaveServer(SocketGuildUser user)
        {
            CacheService.ClearCache(user.Id);
            await Task.CompletedTask;
        }
    }
}
