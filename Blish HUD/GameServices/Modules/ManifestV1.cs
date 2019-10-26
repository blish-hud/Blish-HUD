using Gw2Sharp.WebApi.V2.Models;
using JsonSubTypes;

namespace Blish_HUD.Modules {

    public class ManifestV1 : Manifest {

        /// <inheritdoc />
        public override SupportedModuleManifestVersion ManifestVersion => SupportedModuleManifestVersion.V1;

    }
}
