using System;
using Newtonsoft.Json.Linq;
using SemVer;

namespace Blish_HUD.Modules {

    public class ManifestV1 : Manifest {

        /// <inheritdoc />
        public override SupportedModuleManifestVersion ManifestVersion => SupportedModuleManifestVersion.V1;

    }
}
