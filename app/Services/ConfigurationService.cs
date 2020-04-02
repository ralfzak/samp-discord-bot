using System;
using System.IO;
using Newtonsoft.Json;

namespace app.Services
{
    public class ConfigurationService
    {
        public const string CONFIG_KEY_BOT_TOKEN = "botToken";
        public const string CONFIG_KEY_GUILD_ID = "guildId";
        public const string CONFIG_KEY_VERIFIED_ROLE_ID = "verifiedRoleId";
        public const string CONFIG_KEY_SCRIPTING_CHAN_ID = "scriptingChannelId";
        public const string CONFIG_KEY_ADVERT_CHAN_ID = "advertChannelId";
        public const string CONFIG_KEY_BOT_CHAN_ID = "botChannelId";
        public const string CONFIG_KEY_ADMIN_CHAN_ID = "adminChannelId";
        public const string CONFIG_KEY_DB_SERVER = "dbServer";
        public const string CONFIG_KEY_DB_DB = "dbDatabase";
        public const string CONFIG_KEY_DB_USER = "dbUser";
        public const string CONFIG_KEY_DB_PASS = "dbPassword";
        
        public static string GetConfigurationString(string key)
        {
            dynamic configuration;
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                configuration = JsonConvert.DeserializeObject(json);
            }

            try
            {
                return configuration[key];
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}