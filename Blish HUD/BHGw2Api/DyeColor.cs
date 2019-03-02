using Flurl.Http;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Blish_HUD.BHGw2Api.Cache;
using Humanizer;
using ProtoBuf;

namespace Blish_HUD.BHGw2Api {

    [JsonObject, Serializable, ProtoContract]
    public class Gw2Color {
        [ProtoMember(11)]
        public int R { get; set; }

        [ProtoMember(12)]
        public int G { get; set; }

        [ProtoMember(13)]
        public int B { get; set; }

        [NonSerialized, ProtoIgnore]
        private Color? cachedXnaColor;

        public Gw2Color() { /* NOOP */ }

        public Gw2Color(int r, int g, int b) {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public Color ToXnaColor() {
            // XNA colors are lazy-loaded
            return cachedXnaColor ?? (cachedXnaColor = Color.FromNonPremultiplied(this.R, this.G, this.B, 255)).Value;
        }
    }

    [JsonObject, Serializable, ProtoContract]
    public class DyeMaterialAppearance {

        [ProtoMember(11)]
        public int Brightness { get; set; }

        [ProtoMember(12)]
        public float Contrast { get; set; }

        [ProtoMember(13)]
        public int Hue { get; set; }

        [ProtoMember(14)]
        public float Saturation { get; set; }

        [ProtoMember(15)]
        public float Lightness { get; set; }

        [JsonConverter(typeof(Converters.ColorConverter))]
        [ProtoMember(16)]
        public Gw2Color Rgb { get; set; }

    }

    [JsonObject, Serializable, ProtoContract]
    public class DyeColor : ApiItem {

        [ProtoMember(21)]
        public int Id { get; set; }

        [ProtoMember(22)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "base_rgb"), JsonConverter(typeof(Converters.ColorConverter))]
        [ProtoMember(23)]
        public Gw2Color BaseRgb { get; set; }
        
        [ProtoMember(24)]
        public DyeMaterialAppearance Cloth { get; set; }

        [ProtoMember(25)]
        public DyeMaterialAppearance Leather { get; set; }

        [ProtoMember(26)]
        public DyeMaterialAppearance Metal { get; set; }

        [ProtoMember(27)]
        public DyeMaterialAppearance Fur { get; set; }

        public static List<int> ColorIdIndex;

        public static async Task<DyeColor> GetById(int id) {
            // TODO: Cache times should probably be defined in such a way that the base class just pass them through when it gets the call
            return await GetAsync<DyeColor>("/v2/colors", id.ToString(), GetDyeColorById, DateTimeOffset.Now.Add(4.Days()), CacheDurationType.Sliding, true, true);
        }

        private static async Task<DyeColor> GetDyeColorById(string identifier, string @namespace) {
            if (ColorIdIndex.Contains(Int32.Parse(identifier)))
                return await BASE_API_URL.WithEndpoint(@namespace).ById(identifier).WithTimeout(Settings.TimeoutLength).GetJsonAsync<DyeColor>();

            return null;
        }

        public static void IndexEndpoint(bool fullPreCache = false) {
            Task<List<int>> indexIn = GetAsync<List<int>>("/v2/colors", "index", GetDyeIndex, DateTime.Now.ToDateTimeOffset(14.Hours()), CacheDurationType.Sliding, true, true);
            indexIn.Wait(Settings.TimeoutLength);

            if (!indexIn.IsFaulted)
                ColorIdIndex = indexIn.Result;

            // TODO: This needs to be handled a different way - this function will be depricated shortly (it works, but it's old)
            // Also, all this is accomplishing is the caching
            CallForManyAsync<DyeColor>("/v2/colors?ids=all", 5.Days(), true);
        }

        private static async Task<List<int>> GetDyeIndex(string identifier, string @namespace) {
            return await BASE_API_URL.WithEndpoint(@namespace).WithTimeout(Settings.TimeoutLength).GetJsonAsync<List<int>>();
        }

        public override string CacheKey() => this.Id.ToString();
    }
}
