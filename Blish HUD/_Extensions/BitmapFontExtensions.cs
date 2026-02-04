using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {
    public static class BitmapFontExtensions {

        /// <summary>
        /// Measures the "Logical" size of the string. 
        /// Unlike standard MeasureString, this includes the 'Advance' of trailing whitespace.
        /// </summary>
        public static Vector2 MeasureStringLogical(this BitmapFont font, string text) {
            if (string.IsNullOrEmpty(text))
                return Vector2.Zero;

            var size = Vector2.Zero;
            var currentX = 0f;

            for (var i = 0; i < text.Length; i++) {
                var c = text[i];

                // Handle Newline
                if (c == '\n') {
                    size.X = Math.Max(size.X, currentX);
                    size.Y += font.LineHeight;
                    currentX = 0;
                    continue;
                }

                var region = font.GetCharacterRegion(c);
                if (region == null)
                    continue;

                // Use XAdvance (logical width) instead of TextureRegion.Width (visual width)
                currentX += region.XAdvance + font.LetterSpacing;

                if (BitmapFont.UseKernings && i < text.Length - 1) {
                    if (region.Kernings.TryGetValue(text[i + 1], out var kerning)) {
                        currentX += kerning;
                    }
                }
            }

            size.X = Math.Max(size.X, currentX);
            size.Y += font.LineHeight; // Ensure at least one line height

            return size;
        }
    }
}