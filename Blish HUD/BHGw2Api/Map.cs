using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api.Cache;
using Blish_HUD.Utils;
using Flurl.Http;
using Humanizer;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ProtoBuf;

namespace Blish_HUD.BHGw2Api {
    
    [JsonObject, Serializable, ProtoContract]
    public class Map : ApiItem {

        [ProtoMember(21)]
        public string Id { get;  set; }

        [ProtoMember(22)]
        public string Name { get;  set; }

        [JsonProperty(PropertyName = "Min_Level")]
        [ProtoMember(23)]
        public int MinimumLevel { get;  set; }

        [JsonProperty(PropertyName = "Max_Level")]
        [ProtoMember(24)]
        public int MaximumLevel { get;  set; }

        [JsonProperty(PropertyName = "Default_Floor")]
        [ProtoMember(25)]
        public int DefaultFloor { get;  set; }

        [JsonProperty(PropertyName = "Label_Coord"), JsonConverter(typeof(Converters.PointConverter))]
        [ProtoMember(26)]
        public Point LabelCoordinates { get;  set; }

        [JsonProperty(PropertyName = "Map_Rect"), JsonConverter(typeof(Converters.RectangleConverter))]
        [ProtoMember(27)]
        public Rectangle MapRectangle { get;  set; }

        [JsonProperty(PropertyName = "Continent_Rect"), JsonConverter(typeof(Converters.RectangleConverter))]
        [ProtoMember(28)]
        public Rectangle ContinentRectangle { get;  set; }
        
        [JsonProperty(PropertyName = "Points_Of_Interest"), JsonConverter(typeof(Converters.ChildItemConverter<Landmark>))]
        [ProtoMember(29)]
        public List<Landmark> Landmarks { get;  set; }

        public override string CacheKey() => this.Id;

        public static List<int> MapIdIndex;

        public static async Task<Map> GetFromId(int id) {
            return await $@"https://api.guildwars2.com/v2/maps/{id}".GetJsonAsync<Map>();
        }

        public static void IndexEndpoint() {
            // TODO: This needs to be handled a different way - this function will be depricated shortly (it works, but it's old)
            CallForManyAsync<Map>("/v2/maps?ids=all", 14.Hours(), true);
        }

    }
}
