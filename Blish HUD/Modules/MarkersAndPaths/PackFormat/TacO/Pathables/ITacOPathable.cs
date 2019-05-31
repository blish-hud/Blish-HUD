using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Pathables {
    public interface ITacOPathable  {

        string Type { get; set; }

        PathingCategory Category { get; set; }

    }
}
