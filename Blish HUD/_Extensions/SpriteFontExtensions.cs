using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;

namespace Blish_HUD {
    internal static class SpriteFontExtensions {

        /// <summary>
        /// Converts a <see cref="SpriteFont"/> to a <see cref="BitmapFont"/>.
        /// </summary>
        /// <param name="font">The <see cref="SpriteFont"/> to convert.</param>
        /// <returns>A <see cref="BitmapFont"/> as result of the conversion.</returns>
        public static BitmapFont ToBitmapFont(this SpriteFont font) {

            var regions = new List<BitmapFontRegion>();

            var glyphs = font.GetGlyphs();

            foreach (var glyph in glyphs.Values) {
                var glyphTextureRegion = new TextureRegion2D(font.Texture,
                                                             glyph.BoundsInTexture.Left,
                                                             glyph.BoundsInTexture.Top,
                                                             glyph.BoundsInTexture.Width,
                                                             glyph.BoundsInTexture.Height);

                var region = new BitmapFontRegion(glyphTextureRegion,
                                                  glyph.Character,
                                                  glyph.Cropping.Left,
                                                  glyph.Cropping.Top,
                                                  (int)glyph.WidthIncludingBearings);

                regions.Add(region);
            }

            return new BitmapFont(Guid.NewGuid().ToString("n"), regions, font.LineSpacing);
        }

    }
}
