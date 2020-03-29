using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using app.Helpers;

namespace app.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;

            LoggerService.Write("Binded the commands events!");
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            if (!ChannelAllowsCommands(message.Channel.Id))
                return;

            var argPos = 0;
            if (!message.HasCharPrefix('/', ref argPos))
                return;

            // user cmd cooldown
            if (UserService.IsUserOnCooldown(rawMessage.Author.Id, ""))
            {
                LoggerService.Write($"[MessageReceivedAsync] user {rawMessage.Author.Username} {rawMessage.Author.Id} " +
                                    $"is on a cooldown, msg blocked: {rawMessage.Content}");
                await Task.CompletedTask;
                return;
            }
            UserService.SetUserCooldown(rawMessage.Author.Id, "", 4);

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            // the command failed
            if (result.Error.Value == CommandError.BadArgCount || result.Error.Value == CommandError.ParseFailed)
            {
                await context.Channel.SendMessageAsync("Invalid command parameters. Check `/help`.");
            }
            else if (result.Error.Value == CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            else if (result.Error.Value == CommandError.ObjectNotFound || 
                     result.Error.Value == CommandError.MultipleMatches)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            else
            {
                await _discord
                    .GetGuild(Program.GUILD_ID)
                    .GetTextChannel(Program.ADMIN_CHAN_ID)
                    .SendMessageAsync($"Failed cmd ({command.Value.Name}) by {context.User.Username} " +
                                      $"- error: [{result.Error.ToString()}] {result.ErrorReason}");
            }
        }

        private bool ChannelAllowsCommands(ulong channelId) 
            => channelId != Program.ADVERT_CHAN_ID;
    }
}
