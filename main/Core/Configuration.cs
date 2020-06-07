using System;
using System.IO;
using Newtonsoft.Json;

namespace domain
{
    public class Configuration
    {
        private dynamic _configuration;

        public Configuration()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                _configuration = JsonConvert.DeserializeObject(json);
            }
        }

        public string GetVariable(string key)
        {
            try
            {
                return _configuration[key];
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
