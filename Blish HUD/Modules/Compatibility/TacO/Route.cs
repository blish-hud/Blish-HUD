using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Blish_HUD.Modules.Compatibility.TacO {
    public struct Route {

        public int MapId;

        public double xpos;
        public double ypos;
        public double zpos;

        public string type;

        public double triggerRange;

        public string Guid;

    }
}
