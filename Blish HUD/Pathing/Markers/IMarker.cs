using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Markers {
    public interface IMarker {

        float MinimumSize { get; set; }
        float MaximumSize { get; set; }

        Texture2D Icon { get; set; }

        string Text { get; set; }

    }
}
