using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;

namespace Blish_HUD.Content {
    /// <summary>
    /// Extends <see cref="MonoGame.Extended.BitmapFonts.BitmapFont"/> to allow disposing of its glyph lookup texture.
    /// </summary>
    public class BitmapFont : MonoGame.Extended.BitmapFonts.BitmapFont, IDisposable {

        private readonly Texture2D _texture;

        /// <summary>
        /// Creates a <see cref="BitmapFont"/> with the provided identifier name, glyph regions, line height, and texture to draw letters from.
        /// </summary>
        /// <param name="name">Name to identify the font with.</param>
        /// <param name="regions">Regions of the glyphs on the <c>texture</c>.</param>
        /// <param name="lineHeight">Line height of the font.</param>
        /// <param name="texture">Lookup texture to draw letters from.</param>
        public BitmapFont(string name, IEnumerable<BitmapFontRegion> regions, int lineHeight, Texture2D texture) : base(name, regions, lineHeight) {
            _texture = texture;
        }

        /// <summary>
        /// Creates a <see cref="BitmapFont"/> with the provided identifier name, glyph regions and line height.
        /// </summary>
        /// <param name="name">Name to identify the font with.</param>
        /// <param name="regions">Regions of the glyphs on the <c>texture</c>.</param>
        /// <param name="lineHeight">Line height of the font.</param>
        public BitmapFont(string name, IReadOnlyList<BitmapFontRegion> regions, int lineHeight) : base(name, regions, lineHeight) {
            _texture = regions[0].TextureRegion.Texture;
        }

        /// <summary>
        /// Disposes the lookup texture of this <see cref="BitmapFont"/> to free memory. Renders this <see cref="BitmapFont"/> unusable.
        /// </summary>
        public void Dispose() {
            _texture?.Dispose();
        }
    }
}
