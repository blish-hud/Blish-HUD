using System.Collections.Generic;
using Gw2Sharp.WebApi.V2.Models;
using JsonSubTypes;
using Newtonsoft.Json;

namespace Blish_HUD.Modules {

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

        // Recommended attributes

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("dependencies"), JsonConverter(typeof(ModuleDependency.VersionDependenciesConverter))]
        public List<ModuleDependency> Dependencies { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; } = "";

        [JsonProperty("author")]
        public ModuleContributor Author { get; private set; }

        [JsonProperty("contributors")]
        public List<ModuleContributor> Contributors { get; private set; }

        // Optional attributes

        [JsonProperty("directories")]
        public List<string> Directories { get; private set; }

        [JsonProperty("enable_without_gw2")]
        public bool EnabledWithoutGW2 { get; private set; }

        [JsonProperty("api_permissions")]
        public Dictionary<TokenPermission, ModuleApiPermissions> ApiPermissions { get; private set; } = new Dictionary<TokenPermission, ModuleApiPermissions>();

        public Manifest() {
            // Ensure nothing is empty, despite manifest version and contents
            this.Description    = this.Description    ?? "";
            this.Dependencies   = this.Dependencies   ?? new List<ModuleDependency>(0);
            this.Url            = this.Url            ?? "";
            this.Directories    = this.Directories    ?? new List<string>(0);
            this.ApiPermissions = this.ApiPermissions ?? new Dictionary<TokenPermission, ModuleApiPermissions>(0);
        }

    }

}