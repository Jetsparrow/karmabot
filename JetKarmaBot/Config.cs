using System.IO;
using Newtonsoft.Json;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JetKarmaBot
{
    public class Config : ConfigBase
    {
        public Config(string path) : base(path) { }

        public string ApiKey { get; private set; }
        public string ConnectionString { get; private set; }

        public class ProxySettings
        {
            public string Url { get; private set; }
            public int Port { get; private set; }
            public string Login { get; private set; }
            public string Password { get; private set; }
        }

        public ProxySettings Proxy { get; private set; }
        public class TimeoutConfig
        {
            public int DebtLimitSeconds { get; private set; } = 60 * 60 * 2;
            public Dictionary<string, int> CommandCostsSeconds { get; private set; } = new Dictionary<string, int>()
            {
                {"JetKarmaBot.Commands.AwardCommand (OK)", 60*15},
                {"JetKarmaBot.Commands.AwardCommand (ERR)", 60*5},
                {"Default", 60*5}
            };
            public int SaveIntervalSeconds { get; private set; } = 60 * 5;
            public double AwardTimeSeconds { get; private set; } = 60;
        }
        public TimeoutConfig Timeout { get; private set; } = new TimeoutConfig();
        public bool SqlDebug { get; private set; } = false;
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

