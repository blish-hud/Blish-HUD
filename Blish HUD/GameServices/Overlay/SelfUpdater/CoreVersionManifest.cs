using Newtonsoft.Json;
using Version = SemVer.Version;

namespace Blish_HUD.Overlay.SelfUpdater {
    public struct CoreVersionManifest {

        [JsonProperty("url", Required = Required.Always)]
        public string Url { get; private set; }


        [JsonProperty("checksum", Required = Required.Always)]
        public string Checksum { get; private set; }


        [JsonProperty("version", Required = Required.Always), JsonConverter(typeof(Content.Serialization.SemVerConverter))]
        public Version Version { get; private set; }

        [JsonProperty("is_prerelease", Required = Required.DisallowNull)]
        public bool IsPrerelease { get; private set; }

        [JsonProperty("changelog")]
        public string Changelog { get; private set; }

    }
}