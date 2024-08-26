using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MigrateToJsps
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class Configuration
    {
        /// <summary>
        /// The friendly name of the launch configuration
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of configuration (chrome, edge, or node)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The debug request type ("launch" or "attach")
        /// </summary>
        [JsonProperty("request")]
        public string Request { get; set; }

        /// <summary>
        /// The url to open a browser to
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// User data directory to use when launching browser
        /// </summary>
        [JsonProperty("userDataDir")]
        public object UserDataDir { get; set; }

        /// <summary>
        /// This specifies the workspace absolute path to the webserver root. Used to resolve paths like `/app.js` to files on disk. Shorthand for a pathMapping for "/"
        /// </summary>
        [JsonProperty("webRoot")]
        public string WebRoot { get; set; }

        /// <summary>
        /// Optional current working directory for the runtime executable.
        /// </summary>
        [JsonProperty("cwd")]
        public string Cwd { get; set; }

        /// <summary>
        /// This specifies the workspace absolute path to the program being debugged (currently only applies to Node launch configurations)
        /// </summary>
        [JsonProperty("program")]
        public string Program { get; set; }

        /// <summary>
        /// Tells the debugger which files to skip over 
        /// </summary>
        [JsonProperty("skipFiles")]
        public string[] Skipfiles { get; set; }

        /// <summary>
        /// Automatically stops the program after launch (only applies to Node launch configurations)
        /// </summary>
        [JsonProperty("stopOnEntry")]
        public bool? StopOnEntry { get; set; }

        /// <summary>
        /// Specifies which console -- only used for Node
        /// </summary>
        [JsonProperty("console")]
        public string Console { get; set; }

        /// <summary>
        /// Environment variables passed to the program
        /// </summary>
        [JsonProperty("env")]
        public JObject Env { get; set; }

        /// <summary>
        /// List of arguments passed to the program on the "Program" property
        /// </summary>
        [JsonProperty("args")]
        public string[] Args { get; set; }

        /// <summary>
        /// Other configuration properties which aren't explicitly listed in this class
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JToken> ConfigurationProperties { get; private set; }

        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public bool IsEqualToJson(JToken jsonToCompare)
        {
            JToken config = JToken.FromObject(this);
            return JToken.DeepEquals(config, jsonToCompare);
        }
    }
}
