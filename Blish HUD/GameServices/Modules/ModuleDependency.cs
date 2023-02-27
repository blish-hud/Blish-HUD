﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace Blish_HUD.Modules {

    public class ModuleDependency {

        private const string BLISHHUD_DEPENDENCY_NAME = "bh.blishhud";

        internal class VersionDependenciesConverter : JsonConverter<List<ModuleDependency>> {

            public override void WriteJson(JsonWriter writer, List<ModuleDependency> value, JsonSerializer serializer) {
                writer.WriteStartObject();

                foreach (var dependency in value) {
                    writer.WritePropertyName(dependency.Namespace);
                    writer.WriteValue(dependency.VersionRange.ToString());
                }

                writer.WriteEndObject();
            }

            public override List<ModuleDependency> ReadJson(JsonReader reader, Type objectType, List<ModuleDependency> existingValue, bool hasExistingValue, JsonSerializer serializer) {
                if (reader.TokenType == JsonToken.Null) return null;

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

        public bool IsBlishHud => string.Equals(this.Namespace, BLISHHUD_DEPENDENCY_NAME, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Calculates the current details of the dependency.
        /// </summary>
        public ModuleDependencyCheckDetails GetDependencyDetails() {
            // Check against Blish HUD version
            if (this.IsBlishHud) {
                bool satisfied = this.VersionRange.IsSatisfied(Program.OverlayVersion.BaseVersion());

                // This is a bit scuffed - the idea here is that we only want to evaluate this like a prerelease if the version range intends to target one
                // Otherwise, we don't want to check this way since it will make non-prerelease tags on this version to evaluate as satisfied
                if (Program.OverlayVersion.PreRelease != null && this.VersionRange.ToString().Contains("-ci")) {
                    satisfied &= this.VersionRange.IsSatisfied(Program.OverlayVersion);
                }

                return new ModuleDependencyCheckDetails(this,
                                                        satisfied || Program.OverlayVersion.BaseVersion() == new Version(0, 0, 0) // Ensure local builds ignore prerequisite
                                                            ? ModuleDependencyCheckResult.Available
                                                            : ModuleDependencyCheckResult.AvailableWrongVersion);
            }

            // Check for module dependency
            foreach (var module in GameService.Module.Modules) {
                if (string.Equals(this.Namespace, module.Manifest.Namespace, StringComparison.OrdinalIgnoreCase)) {
                    if (this.VersionRange.IsSatisfied(module.Manifest.Version.BaseVersion())) {
                        // Module exists and is a valid version
                        return new ModuleDependencyCheckDetails(this,
                                                                module.Enabled
                                                                    ? ModuleDependencyCheckResult.Available
                                                                    : ModuleDependencyCheckResult.AvailableNotEnabled,
                                                                module);
                    }

                    // Module exists but is the wrong version
                    return new ModuleDependencyCheckDetails(this, ModuleDependencyCheckResult.AvailableWrongVersion, module);
                }
            }

            // No module could be found that matches
            return new ModuleDependencyCheckDetails(this, ModuleDependencyCheckResult.NotFound);
        }

    }

}