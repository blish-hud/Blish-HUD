using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.TacO.Origin {
    public class POI {

        public MarkerTypeData TypeData;
        // TODO: Determine what WBATLASHANDLE is
        //public WBATLASHANDLE icon;
        public Size IconSize;

        public Vector4 CameraSpacePosition;

        public Vector3 Position;
        public int MapId = 0;
        public string Name;
        public string Type;

        public long LastUpdateTime = 0;
        public bool External = false;
        public bool RouteMember = false;
        public string ZipFile;

        public Guid Guid;

        public int WarningTriggerTime = 0;

        public GW2TacticalCategory Category;

        public void SetCategory(GW2TacticalCategory t) {
            throw new NotImplementedException();
        }
    }
}
