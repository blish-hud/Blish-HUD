using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls.Resources {
    public static class Control {

        public static readonly TextureAtlas TextureAtlasControl;

        static Control() {
            TextureAtlasControl = GameService.Content.GetTextureAtlas(@"atlas\ui");
        }

    }
}
