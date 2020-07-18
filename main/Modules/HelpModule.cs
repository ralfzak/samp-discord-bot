using System.Threading.Tasks;
using main.Core;
using Discord;
using Discord.Commands;

namespace main.Modules
{
    /// <summary>
    /// Encapsulates all help-related commands.
    /// </summary>
    #pragma warning disable 4014,1998    
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly ulong _adminChannelId;
        private readonly ulong _botChannelId;
        private readonly ulong _scriptingChannelId;
        private readonly string _websiteUrl;

        public HelpModule()
        {
            _adminChannelId = Configuration.GetVariable<ulong>(ConfigurationKeys.GuildAdminChannelId);
            _botChannelId = Configuration.GetVariable<ulong>(ConfigurationKeys.GuildBotcommandsChannelId);
            _scriptingChannelId = Configuration.GetVariable<ulong>(ConfigurationKeys.GuildScriptingChannelId);
            _websiteUrl = Configuration.GetVariable(ConfigurationKeys.UrlSampWebsite);
        }

        [Command("help")]
        [Alias("cmds", "commands")]
        public async Task Help()
        {
            if (Context.Channel.Id != _adminChannelId)
            {
                Context.Message.DeleteAsync();
                Context.User.SendMessageAsync("", embed: GetPublicHelpList());
            }
            else
            {
                ReplyAsync("", embed: GetAdminHelpList());
            }
        }

        private Embed GetPublicHelpList()
        {
            var builder = new EmbedBuilder()
                .WithTitle("General Commands")
                .WithDescription("List of general server commands that I respond to:")
                .WithColor(new Color(0xD0021B))
                .AddField("/verify",
                    $"[**Parameters:**]({_websiteUrl}) <profile_id/done/cancel>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) Links your discord account with a SAMP forum account using a simple verification process." +
                    "\n" +
                    $"[**Availability:**]({_websiteUrl}) As a direct message.")
                
                .AddField("/whois",
                    $"[**Parameters:**]({_websiteUrl}) [@]<user>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command looks up SAMP forum profiles of given linked discord accounts." +
                    "\n" +
                    $"[**Availability:**]({_websiteUrl}) On the discord server where I am available.")

                .AddField("/server, /srv",
                    $"[**Parameters:**]({_websiteUrl}) <ip/hostname>[:port]" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command fetches SAMP server data for a given server." +
                    "\n" +
                    $"[**Availability:**]({_websiteUrl}) Only on <#{_botChannelId}>.")

            .AddField("/wiki",
                    $"[**Parameters:**]({_websiteUrl}) <callback/function/article>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command fetches articles from the official SAMP Wiki." +
                    "\n" +
                    $"[**Availability:**]({_websiteUrl}) Only on <#{_botChannelId}> and <#{_scriptingChannelId}>.");

            return builder.Build();
        }

        private Embed GetAdminHelpList()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Admin Commands")
                .WithDescription("List of admin commands that I only respond to here:")
                .WithColor(new Color(0xD0021B))
                .AddField("/fverify",
                    $"[**Parameters:**]({_websiteUrl}) [@]<user> [forumid]" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) Force links a discord account with a SAMP forum account.")

                .AddField("/funverify",
                    $"[**Parameters:**]({_websiteUrl}) [@]<user>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) Drops a forum verification and removes the user role.")

                .AddField("/rvwhois",
                    $"[**Parameters:**]({_websiteUrl}) <forum_id/forum_name>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command fetches discord user(s) linked to a given forum account.")

                .AddField("/ban",
                    $"[**Parameters:**]({_websiteUrl}) [@]<user> [days] [hours] [send ban message] [reason]" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command issues a discord ban. Use this command with no parameters for more information. Check channel pins.")

                .AddField("/banlookup",
                    $"[**Parameters:**]({_websiteUrl}) <userid, username>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command lists a list of banned account based on a search criteria. Check channel pins.")

                .AddField("/unban",
                    $"[**Parameters:**]({_websiteUrl}) <userid, username>" +
                    "\n" +
                    $"[**Info:**]({_websiteUrl}) This command lifts a list of bans given a certain criteria. Check channel pins.");

            return builder.Build();
        }
    }
}
