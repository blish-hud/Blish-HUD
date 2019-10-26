using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SemVer;

namespace Blish_HUD.Modules {

    public class ModuleDependency {

        private const bool   VERSIONRANGE_LOOSE       = false;
        private const string BLISHHUD_DEPENDENCY_NAME = "bh.blishhud";

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
                        VersionRange = new Range(dependencyVersionRange)
                    });
                }

                return moduleDependencyList;
            }
        }

        public string Namespace { get; private set; }

        public Range VersionRange { get; private set; }

        /// <summary>
        /// Calculates the current details of the dependency.
        /// </summary>
        public ModuleDependencyCheckDetails GetDependencyDetails() {
            // Check against Blish HUD version
            if (string.Equals(this.Namespace, BLISHHUD_DEPENDENCY_NAME, StringComparison.OrdinalIgnoreCase)) {
                if (VersionRange.IsSatisfied(Program.OverlayVersion.BaseVersion())) {
                    return new ModuleDependencyCheckDetails("Blish HUD", ModuleDependencyCheckResult.Available);
                }

                return new ModuleDependencyCheckDetails("Blish HUD", ModuleDependencyCheckResult.AvailableWrongVersion);
            }

            // Check for module dependency
            foreach (var module in GameService.Module.Modules) {
                if (string.Equals(this.Namespace, module.Manifest.Namespace, StringComparison.OrdinalIgnoreCase)) {
                    if (this.VersionRange.IsSatisfied(module.Manifest.Version.BaseVersion())) {
                        // Module exists and is a valid version
                        return new ModuleDependencyCheckDetails(module.Manifest.Name,
                                                                module.Enabled
                                                                    ? ModuleDependencyCheckResult.Available
                                                                    : ModuleDependencyCheckResult.AvailableNotEnabled,
                                                                module);
                    }

                    // Module exists but is the wrong version
                    return new ModuleDependencyCheckDetails(module.Manifest.Name, ModuleDependencyCheckResult.AvailableWrongVersion, module);
                }
            }

            // No module could be found that matches
            return new ModuleDependencyCheckDetails(this.Namespace, ModuleDependencyCheckResult.NotFound);
        }

    }

}