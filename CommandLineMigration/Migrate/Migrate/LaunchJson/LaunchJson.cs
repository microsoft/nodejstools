using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NtvsMigration
{
    /// <summary>
    /// Represents a launch.json file
    /// </summary>
    public class LaunchJson
    {
        /// <summary>
        /// The friendly name of the launch configuration
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// The type of configuration (chrome, edge, or node)
        /// </summary>
        [JsonProperty("configurations")]
        Configuration[] Configurations { get; set; } = new Configuration[0];

        /// <summary>
        /// The debug request type ("launch" or "attach")
        /// </summary>
        [JsonProperty("compounds")]
        Compound[] Compounds { get; set; } = new Compound[0];

        /// <summary>
        /// Other launch.json properties which aren't explicitly listed in this class
        /// </summary>
        [Newtonsoft.Json.JsonExtensionData]
        public Dictionary<string, JToken> LaunchJsonProperties { get; private set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

    /// <summary>
    /// Interface to represent any option shown to the user in the debug dropdown in VS 
    /// </summary>
    public interface IDebugConfig
    {
        string Name { get; set; }

        string ToJsonString();

        bool IsHidden();
    }
}
