using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD.BHGw2Api {
    
    [JsonObject]
    public class Continent {

        public string Id { get; set; }
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Continent_Dims"), JsonConverter(typeof(Converters.PointConverter))]
        public Point ContinentDimensions { get; set; }

        [JsonProperty(PropertyName = "Min_Zoom")]
        public int MinimumZoom { get; set; }

        [JsonProperty(PropertyName = "Max_Zoom")]
        public int MaximumZoom { get; set; }
        public int[] Floors { get; set; }

    }
}
