using Newtonsoft.Json;

namespace Blish_HUD.Modules {

    public class ManifestV1 : Manifest {

        [JsonProperty("manifest_version")]
        public override SupportedModuleManifestVersion ManifestVersion => SupportedModuleManifestVersion.V1;

    }
}
