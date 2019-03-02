using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Markers {

    public enum AugTyrNodeType {
        Normal = 0,
        Waypoint = 1,
        Comment = 2,
        HeartWall = 3
    }

    public class AugTyrNode {

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public string Comment { get; set; }
        public AugTyrNodeType Type { get; set; }
        public string WaypointCode { get; set; }
        public string HeartWallValue { get; set; }

    }
}
