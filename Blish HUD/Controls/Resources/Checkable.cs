using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls.Resources {
    public static class Checkable {

        public static readonly List<TextureRegion2D> TextureRegionsCheckbox;

        static Checkable() {
            TextureRegionsCheckbox = new List<TextureRegion2D>();

            TextureRegionsCheckbox.AddRange(new[] {
                                                Control.TextureAtlasControl.GetRegion("checkbox/cb-unchecked"),
                                                Control.TextureAtlasControl.GetRegion("checkbox/cb-unchecked-active"),
                                                Control.TextureAtlasControl.GetRegion("checkbox/cb-unchecked-disabled"),
                                                Control.TextureAtlasControl.GetRegion("checkbox/cb-checked"),
                                                Control.TextureAtlasControl.GetRegion("checkbox/cb-checked-active"),
                                                Control.TextureAtlasControl.GetRegion("checkbox/cb-checked-disabled"),
                                            });
        }

    }
}
