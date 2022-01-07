using Newtonsoft.Json;
using Version = SemVer.Version;

namespace Blish_HUD.Overlay {
    public struct CoreVersionManifest {

        [JsonProperty("url", Required = Required.Always)]
        public string Url { get; set; }


        [JsonProperty("checksum", Required = Required.Always)]
        public string Checksum { get; set; }


        [JsonProperty("version", Required = Required.Always), JsonConverter(typeof(Content.Serialization.SemVerConverter))]
        public Version Version { get; set; }

        [JsonProperty("is_prerelease", Required = Required.DisallowNull)]
        public bool IsPrerelease { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

    }
}