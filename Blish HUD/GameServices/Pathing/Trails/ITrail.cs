using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Trails {
    public interface ITrail {

        IReadOnlyList<Vector3> TrailPoints { get; }

        float TrailLength { get; }

        float DistanceFromPlayer { get; }
        float DistanceFromCamera { get; }

        float Opacity { get; }

    }
}
