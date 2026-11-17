using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Content {
    /// <summary>
    /// Extends <see cref="BitmapFont"/> to allow disposing of its glyph lookup texture.
    /// </summary>
    public class BitmapFontEx : BitmapFont, IDisposable {
        private readonly Texture2D _texture;

        /// <summary>
        /// Creates a <see cref="BitmapFontEx"/> with the provided identifier name, glyph regions, line height, and texture to draw letters from.
        /// </summary>
        /// <param name="name">Name to identify the font with.</param>
        /// <param name="regions">Regions of the glyphs on the <c>texture</c>.</param>
        /// <param name="lineHeight">Line height of the font.</param>
        /// <param name="texture">Lookup texture to draw letters from.</param>
        public BitmapFontEx(string name, IEnumerable<BitmapFontRegion> regions, int lineHeight, Texture2D texture) : base(name, regions, lineHeight) {
            _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        }

        /// <summary>
        /// Creates a <see cref="BitmapFontEx"/> with the provided identifier name, glyph regions and line height.
        /// </summary>
        /// <param name="name">Name to identify the font with.</param>
        /// <param name="regions">Regions of the glyphs on the <c>texture</c>.</param>
        /// <param name="lineHeight">Line height of the font.</param>
        public BitmapFontEx(string name, IReadOnlyList<BitmapFontRegion> regions, int lineHeight) : base(name, regions, lineHeight) {
            _texture = regions?.FirstOrDefault()?.TextureRegion?.Texture ?? throw new ArgumentException($"Parameter '{nameof(regions)}' was null or empty.");
        }

        /// <summary>
        /// Disposes the lookup texture of this <see cref="BitmapFontEx"/> to free memory. Renders this <see cref="BitmapFontEx"/> unusable.
        /// </summary>
        public void Dispose() {
            _texture?.Dispose();
        }
    }
}
