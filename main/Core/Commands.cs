using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using main.Helpers;
using main.Services;

namespace main.Core
{
    #pragma warning disable 4014,1998
    public class Commands
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly UserService _userService;
        private readonly MessageService _messageService;

        public Commands(IServiceProvider services, CommandService commands, DiscordSocketClient discord, MessageService messageService, UserService userService)
        {
            _services = services;
            _userService = userService;
            _messageService = messageService;
            _commands = commands;
            _discord = discord;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
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

            var argPos = 0;
            if (!message.HasCharPrefix('/', ref argPos))
                return;

            if (_userService.IsUserOnCooldown(rawMessage.Author.Id))
                return;

            _userService.SetUserCooldown(rawMessage.Author.Id, "", 4);
            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            if (result.Error.Value == CommandError.BadArgCount || result.Error.Value == CommandError.ParseFailed)
            {
                var response = await context.Channel.SendMessageAsync("Invalid command parameters. Check `/help`.");
                _messageService.LogCommand(context.Message.Id, response.Id);
            }
            else if (result.Error.Value == CommandError.UnmetPrecondition)
            {
                var response = await context.Channel.SendMessageAsync(result.ErrorReason);
                _messageService.LogCommand(context.Message.Id, response.Id);
            }
            else if (result.Error.Value == CommandError.ObjectNotFound || 
                     result.Error.Value == CommandError.MultipleMatches)
            {
                var response = await context.Channel.SendMessageAsync(result.ErrorReason);
                _messageService.LogCommand(context.Message.Id, response.Id);
            }
            else
            {
                Logger.Write($"[{command.Value.Name}] Failed: {context.User.Username} - [{result.Error.ToString()}] {result.ErrorReason}");
            }
        }
    }
}
