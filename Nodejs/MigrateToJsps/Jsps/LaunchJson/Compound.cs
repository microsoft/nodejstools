using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrateToJsps
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class Compound
    {
        /// <summary>
        /// The friendly name of the compound configuration
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A string array containing the friendly names of the launch configs that this compound will start
        /// </summary>
        [JsonProperty("configurations")]
        public string[] Configurations { get; set; }

        /// <summary>
        /// Indicates whether terminating one session will terminate all debugging sessions
        /// </summary>
        [JsonProperty("stopAll")]
        public bool? StopAll { get; set; }

        /// <summary>
        /// Task to run before any of the compound configurations start
        /// </summary>
        [JsonProperty("preLaunchTask")]
        public string PreLaunchTask { get; set; }

        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public bool IsEqualToJson(JToken jsonToCompare)
        {
            JToken config = JToken.FromObject(this);
            return JToken.DeepEquals(config, jsonToCompare);
        }
    }
}
