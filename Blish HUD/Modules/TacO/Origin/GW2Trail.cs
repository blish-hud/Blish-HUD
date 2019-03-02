using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Modules.TacO.Origin {
    public class GW2Trail {

        private List<Vector3> Positions;

        public void Reset(int _mapID = 0) {
            throw new NotImplementedException();
        }

        public bool SaveToFile(string fname) {
            throw new NotImplementedException();
        }

        public int Length = 0;

        // --- Refer to TrailLogger.h for additional members to implement --- //

        public MarkerTypeData TypeData;
        public string Type;
        public Guid Guid;
        public bool External = false;
        public string ZipFile;

        private GW2TacticalCategory Category = null;

        // --- Refer to TrailLogger.h for additional methods to implement --- //

    }
}
