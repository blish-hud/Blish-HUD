using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Trails {

    /// <summary>
    /// Uses a series of dots to guide the player along a route.  When a
    /// player comes to the next dot, it will disapear until the player
    /// goes back to the beginning of the route.
    /// 
    /// Meant to match the functionality of routes imported in from TacO.
    /// </summary>
    [Obsolete("This path type is implemented primarily to match TacO 'route' functionality."
            + "You may prefer to utilize 'StandardTrail' or 'FollowablePath', both of which"
            + "provide a more obvious path")]
    public class Route : ITrail {

        public IReadOnlyList<Vector3> TrailPoints { get; }
        public float TrailLength { get; }

        public float DistanceFromPlayer { get; }
        public float DistanceFromCamera { get; }

        public float Opacity { get; }

    }
}
