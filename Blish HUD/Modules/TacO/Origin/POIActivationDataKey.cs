using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.TacO.Origin {
    public class POIActivationDataKey {

        public Guid Guid;
        public int UniqueData = 0;

        public POIActivationDataKey(Guid g, int inst) {
            this.Guid = g;
            this.UniqueData = inst;
        }

        public override bool Equals(object obj) {
            var d = (POIActivationDataKey) obj;
            if (d == null) return false;

            return (this.Guid == d.Guid && this.UniqueData == d.UniqueData);
        }

    }
}
