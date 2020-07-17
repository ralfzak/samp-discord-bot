using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace main.Core
{
    /// <summary>
    /// General configuration interaction.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Returns configuration variables from set environment variables.
        /// </summary>
        /// <param name="key">A configuration key</param>
        /// <returns>The key value casted as a <see cref="T"/>, or a null if key was not found</returns>
        public static T GetVariable<T>(ConfigurationKeys key)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.UInt64:
                    return (T)(object)UInt64.Parse(Environment.GetEnvironmentVariable(key.Value) ?? "");
                
                case TypeCode.String:
                    return (T)(object)(Environment.GetEnvironmentVariable(key.Value) ?? "");
                
                case TypeCode.Char:
                    return (T)(object)(Environment.GetEnvironmentVariable(key.Value) ?? "").ElementAt(0);                
            }

            return (T)(object)null;
        }
        
        /// <summary>
        /// Returns configuration variables from set environment variables.
        /// </summary>
        /// <param name="key">A configuration key</param>
        /// <returns>The key value as a string, or a null if key was not found</returns>
        public static string GetVariable(ConfigurationKeys key) => GetVariable<string>(key);
    }

    /// <summary>
    /// Defines the global set of configuration keys along with their values
    /// </summary>
    public class ConfigurationKeys
    {
        public string Value { get; }
        
        private ConfigurationKeys(string value)
        {
            Value = value;
        }
        
        public static ConfigurationKeys BotToken = new ConfigurationKeys("BOT_TOKEN");
        public static ConfigurationKeys BotId = new ConfigurationKeys("BOT_ID");
        public static ConfigurationKeys GuildId = new ConfigurationKeys("GUILD_ID");
        public static ConfigurationKeys GuildScriptingChannelId = new ConfigurationKeys("GUILD_SCRIPTING_CHANNEL_ID");
        public static ConfigurationKeys GuildBotcommandsChannelId = new ConfigurationKeys("GUILD_BOTCOMMANDS_CHANNEL_ID");
        public static ConfigurationKeys GuildAdminChannelId = new ConfigurationKeys("GUILD_ADMIN_CHANNEL_ID");
        public static ConfigurationKeys GuildVerifiedRoleId = new ConfigurationKeys("GUILD_VERIFIED_ROLE_ID");
        public static ConfigurationKeys GuildCommandPrefix = new ConfigurationKeys("GUILD_COMMAND_PREFIX");
        public static ConfigurationKeys UrlSampWebsite = new ConfigurationKeys("URL_SAMP_WEBSITE");
        public static ConfigurationKeys UrlSampHostedtabprovider = new ConfigurationKeys("URL_SAMP_HOSTEDTABPROVIDER");
        public static ConfigurationKeys UrlForumProfile = new ConfigurationKeys("URL_FORUM_PROFILE");
        public static ConfigurationKeys UrlForumSettings = new ConfigurationKeys("URL_FORUM_SETTINGS");
        public static ConfigurationKeys UrlWikiDocs = new ConfigurationKeys("URL_WIKI_DOCS");
        public static ConfigurationKeys UrlWikiSearch = new ConfigurationKeys("URL_WIKI_SEARCH");
        public static ConfigurationKeys DatabaseHost = new ConfigurationKeys("DATABASE_HOST");
        public static ConfigurationKeys DatabaseSchema = new ConfigurationKeys("DATABASE_SCHEMA");
        public static ConfigurationKeys DatabaseUser = new ConfigurationKeys("DATABASE_USER");
        public static ConfigurationKeys DatabasePassword = new ConfigurationKeys("DATABASE_PASSWORD");
    }
}
