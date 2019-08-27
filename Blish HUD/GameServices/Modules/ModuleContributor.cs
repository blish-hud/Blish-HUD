using Newtonsoft.Json;

namespace Blish_HUD.Modules {

    public class ModuleContributor {

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

    }

}