using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace main.Core
{
    public static class Configuration
    {
        /**
         * Returns configuration variables from config.json file.
         */
        public static dynamic GetVariable(string key)
        {
            try
            {
                using (StreamReader r = new StreamReader("config.json"))
                {
                    string json = r.ReadToEnd();
                    dynamic deserializeObject = JsonConvert.DeserializeObject(json);
                    
                    if (deserializeObject is string)
                        return deserializeObject[key];

                    if (key.Contains('.'))
                    {
                        var keys = key.Split('.');
                        var reference = deserializeObject;
                        for (int i = 0; i < keys.Count(); i++)
                        {
                            deserializeObject = reference[keys[i]];
                            reference = deserializeObject;
                        }

                        return reference;
                    }

                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
