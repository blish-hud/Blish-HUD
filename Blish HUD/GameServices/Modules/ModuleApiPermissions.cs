using Newtonsoft.Json;

namespace Blish_HUD.Modules {

    public class ModuleApiPermissions {

        [JsonProperty("optional")]
        public bool Optional { get; private set; }

        [JsonProperty("details")]
        public string Details { get; private set; }

    }

}