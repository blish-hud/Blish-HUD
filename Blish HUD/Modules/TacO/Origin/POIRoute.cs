using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.TacO.Origin {
    public class POIRoute {

        public string Name;
        public bool Backwards;
        public Guid[] Route;
        public bool External;
        public bool HasResetPos;
        public Vector3 ResetPos;
        public float ResetRad;
        public int MapId;

        public int ActiveItem;

    }
}
