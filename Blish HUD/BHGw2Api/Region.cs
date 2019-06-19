using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD.BHGw2Api {

    [JsonObject]
    public class Region : ApiItem {

        public string Id { get; set; }
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Label_Coord"), JsonConverter(typeof(Converters.PointConverter))]
        public Point LabelCoordinates { get; set; }

        [JsonProperty(PropertyName = "Continent_Rect"), JsonConverter(typeof(Converters.RectangleConverter))]
        public Rectangle ContinentRectangle { get; set; }

        [JsonConverter(typeof(Converters.ChildItemConverter<Map>))]
        public List<Map> Maps { get; set; }

        public override string CacheKey() => this.Id;
    }
}
