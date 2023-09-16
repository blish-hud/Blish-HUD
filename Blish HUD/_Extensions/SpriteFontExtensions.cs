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
        /// <param name="lineHeight">Line height for the <see cref="BitmapFont"/>. By default, <see cref="SpriteFont.LineSpacing"/> will be used.</param>
        /// <returns>A <see cref="BitmapFont"/> as result of the conversion.</returns>
        public static BitmapFont ToBitmapFont(this SpriteFont font, int lineHeight = 0) {
            if (lineHeight < 0) {
                throw new ArgumentException("Line height cannot be negative.", nameof(lineHeight));
            }

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

            return new BitmapFont($"{typeof(BitmapFont)}_{Guid.NewGuid():n}", regions, lineHeight > 0 ? lineHeight : font.LineSpacing);
        }

    }
}
