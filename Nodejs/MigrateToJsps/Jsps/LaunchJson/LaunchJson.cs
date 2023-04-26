using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace MigrateToJsps
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class LaunchJson
    {
        /// <summary>
        /// The friendly name of the launch configuration
        /// </summary>
        [JsonProperty("version")]
        public string Version = "0.2.0";

        /// <summary>
        /// The type of configuration (chrome, edge, or node)
        /// </summary>
        [JsonProperty("configurations")]
        public Configuration[] Configurations { get; set; }

        /// <summary>
        /// The debug request type ("launch" or "attach")
        /// </summary>
        [JsonProperty("compounds")]
        public Compound[] Compounds { get; set; }

        /// <summary>
        /// Other launch.json properties which aren't explicitly listed in this class
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JToken> LaunchJsonProperties { get; private set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

}
