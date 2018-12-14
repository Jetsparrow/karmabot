using System;
using System.IO;
using Newtonsoft.Json;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json.Linq;

namespace JetKarmaBot
{
    public class Config : ConfigBase
    {
        public Config(string path) : base(path) { }

        public string ApiKey { get; private set; }
        public string ConnectionString { get; private set; }
        public string ProxyUrl { get; private set; }
        public int ProxyPort { get; private set; }
        public string ProxyLogin { get; private set; }
        public string ProxyPassword { get; private set; }
    }

    public abstract class ConfigBase
    {
        public ConfigBase(string path)
        {
            JObject configJson;

            if (File.Exists(path))
            {
                configJson = JObject.Parse(File.ReadAllText(path));

                using (var sr = configJson.CreateReader())
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new PrivateSetterContractResolver()
                    };
                    JsonSerializer.CreateDefault(settings).Populate(sr, this);
                }
            }
            else configJson = new JObject();

            configJson.Merge(JToken.FromObject(this), new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            try // populate possible missing properties in file
            {
                File.WriteAllText(path, configJson.ToString(Formatting.Indented));
            }
            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
    }
}
            
