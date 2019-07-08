using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Trails {
    public interface ITrail {

        IReadOnlyList<Vector3> TrailPoints { get; }

        float TrailLength { get; }

        float DistanceFromPlayer { get; }
        float DistanceFromCamera { get; }

        float Opacity { get; }

    }
}
