using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.TacO.Origin {
    public class GW2TacticalCategory {

        public string CachedTypeName;

        public string Name;
        public string DisplayName;
        public MarkerTypeData Data;
        public bool KeepSaveState = false;
        public GW2TacticalCategory Parent;
        public List<GW2TacticalCategory> Children;

        public bool IsDisplayed = true;

        public bool IsVisible() {
            throw new NotImplementedException();
        }

        public string GetFullTypeName() {
            throw new NotImplementedException();
        }

        public GW2TacticalCategory() {
            Children = new List<GW2TacticalCategory>();
        }

    }
}
