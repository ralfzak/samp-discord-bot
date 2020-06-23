using System;
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
        /// Returns configuration variables from config.json file.
        /// </summary>
        /// <param name="key">The Json Key, dot delimited, that identifies for a given value</param>
        /// <returns>The key value set, or null if no matching key is found</returns>
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
