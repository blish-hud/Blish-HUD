using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SemVer;

namespace Blish_HUD.Modules {

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

}