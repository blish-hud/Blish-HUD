using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ProtoBuf;

namespace Blish_HUD.BHGw2Api {

    public enum LandmarkType {
        Poi = 0,
        Vista = 1,
        Waypoint = 2,
        HeroPoint = 3,
        Dungeon = 4,
        Raid = 5,
        JumpingPuzzle = 6,
        Adventure = 7
    }

    [JsonObject]
    [ProtoContract]
    public class Landmark : ApiItem {
        
        [ProtoMember(21)]
        public string Id { get; set; }

        [ProtoMember(22)]
        public string Name { get; set; }

        [ProtoMember(23)]
        public string Type { get; set; }

        [ProtoMember(24)]
        public int Floor { get; set; }

        [JsonProperty(PropertyName = "Coord"), JsonConverter(typeof(Converters.PointConverter))]
        [ProtoMember(25)]
        public Point Coordinates { get; set; }

        [JsonProperty(PropertyName = "Chat_Link")]
        [ProtoMember(26)]
        public string ChatLink { get; set; }
        
        [ProtoMember(27)]
        public string Icon { get; set; }

        // TODO: Add support for something like this:
        //public Landmark GetClosestWaypoint(Point coords) {
        //    BHGw2Api.Landmark closestWp = null;
        //    float distance = float.MaxValue;

        //    var staticPos = new Vector2((float)landmark.Coordinates.X, (float)landmark.Coordinates.Y);

        //    foreach (var wp in PointsOfInterest.Values.Where(poi => poi.Type == "waypoint" && landmark != poi)) {
        //        var pos = new Vector2((float)wp.Coordinates.X, (float)wp.Coordinates.Y);

        //        var netDist = Vector2.Distance(staticPos, pos);

        //        if (netDist < distance) {
        //            closestWp = wp;
        //            distance = netDist;
        //        }
        //    }

        //    return closestWp;
        //}

        public override string CacheKey() => this.Id;
    }
}
