using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using JsonSubTypes;
using Newtonsoft.Json;

namespace Blish_HUD.Modules {

    public enum SupportedModuleManifestVersion : int {
        V1 = 1,
    }

    public enum TempPerm {
        Account,
        Inventories,
        Characters,
        TradingPost,
        Wallet,
        Unlocks,
        PvP,
        Builds,
        Progression,
        Guilds
    }

    public class ModuleDependecy {

        public string Namespace { get; private set; }

        public SemVer.Range VersionRange { get; private set; }

    }

    public class Contributer {

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

    }

    public class APIPermissions {

        [JsonProperty("optional")]
        public bool Optional { get; private set; }

        [JsonProperty("details")]
        public string Details { get; private set; }

    }

    [JsonConverter(typeof(JsonSubtypes), "manifest_version")]
    [JsonSubtypes.KnownSubType(typeof(ManifestV1), SupportedModuleManifestVersion.V1)]
    public abstract class Manifest {
        public abstract SupportedModuleManifestVersion ManifestVersion { get; }

        // Required attribtes

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty("version", Required = Required.Always), JsonConverter(typeof(Content.Serialization.SemVerConverter))]
        public SemVer.Version Version { get; private set; }

        [JsonProperty("namespace", Required = Required.Always)]
        public string Namespace { get; private set; }

        [JsonProperty("package", Required = Required.Always)]
        public string Package { get; private set; }

    }

    public class ManifestV1 : Manifest {

        /// <inheritdoc />
        public override SupportedModuleManifestVersion ManifestVersion => SupportedModuleManifestVersion.V1;

        // Recommended attributes

        [JsonProperty("description")]
        public string Description { get; private set; }

        public List<ModuleDependecy> Dependencies { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("author")]
        public Contributer Author { get; private set; }

        [JsonProperty("contributers")]
        public List<Contributer> Contributers { get; private set; }

        // Optional attributes

        [JsonProperty("directories")]
        public List<string> Directories { get; private set; }

        [JsonProperty("enable_without_gw2")]
        public bool EnabledWithoutGW2 { get; private set; }

        [JsonProperty("api_permissions")]
        public Dictionary<TempPerm, APIPermissions> APIPermissions { get; private set; }

    }
}
