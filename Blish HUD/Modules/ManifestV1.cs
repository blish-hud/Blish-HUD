using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Gw2Sharp.WebApi.V2.Models;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SemVer;

namespace Blish_HUD.Modules {

    public enum SupportedModuleManifestVersion : int {
        V1 = 1,
    }

    public class ModuleDependency {

        private const bool VERSIONRANGE_LOOSE = false;

        internal class VersionDependenciesConverter : JsonConverter<List<ModuleDependency>> {
            
            public override void WriteJson(JsonWriter writer, List<ModuleDependency> value, JsonSerializer serializer) {
                writer.WriteValue(value.ToString());
            }

            public override List<ModuleDependency> ReadJson(JsonReader reader, Type objectType, List<ModuleDependency> existingValue, bool hasExistingValue, JsonSerializer serializer) {
                var moduleDependencyList = new List<ModuleDependency>();

                JObject mdObj = JObject.Load(reader);

                foreach (var prop in mdObj) {
                    string dependencyNamespace    = prop.Key;
                    string dependencyVersionRange = prop.Value.ToString();

                    moduleDependencyList.Add(new ModuleDependency() {
                        Namespace    = dependencyNamespace,
                        VersionRange = new Range(dependencyVersionRange, VERSIONRANGE_LOOSE)
                    });
                }

                return moduleDependencyList;
            }
        }

        public string Namespace { get; private set; }

        public SemVer.Range VersionRange { get; private set; }

    }

    public class Contributor {

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

    }

    public class ApiPermissions {

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

        // Recommended attributes

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("dependencies"), JsonConverter(typeof(ModuleDependency.VersionDependenciesConverter))]
        public List<ModuleDependency> Dependencies { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; } = "";

        [JsonProperty("author")]
        public Contributor Author { get; private set; }

        [JsonProperty("contributors")]
        public List<Contributor> Contributors { get; private set; }

        // Optional attributes

        [JsonProperty("directories")]
        public List<string> Directories { get; private set; }

        [JsonProperty("enable_without_gw2")]
        public bool EnabledWithoutGW2 { get; private set; }

        [JsonProperty("api_permissions")]
        public Dictionary<TokenPermission, ApiPermissions> ApiPermissions { get; private set; } = new Dictionary<TokenPermission, ApiPermissions>();

        public Manifest() {
            // Ensure nothing is empty, despite manifest version and contents
            this.Description    = this.Description    ?? "";
            this.Dependencies   = this.Dependencies   ?? new List<ModuleDependency>(0);
            this.Url            = this.Url            ?? "";
            this.Directories    = this.Directories    ?? new List<string>(0);
            this.ApiPermissions = this.ApiPermissions ?? new Dictionary<TokenPermission, ApiPermissions>(0);
        }

    }

    public class ManifestV1 : Manifest {

        /// <inheritdoc />
        public override SupportedModuleManifestVersion ManifestVersion => SupportedModuleManifestVersion.V1;

    }
}
