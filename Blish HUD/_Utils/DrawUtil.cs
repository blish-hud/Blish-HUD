using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Linq;
using System.Text;
using Blish_HUD.Controls;

namespace Blish_HUD {
    public static class DrawUtil {

        public static void DrawAlignedText(SpriteBatch sb, SpriteFont sf, string text, Rectangle bounds, Color clr, HorizontalAlignment ha, VerticalAlignment va) {
            // Filter out any characters our font doesn't support
            text = string.Join("", text.ToCharArray().Where(c => sf.Characters.Contains(c)));

            var textSize = sf.MeasureString(text);

            int xPos = bounds.X;
            int yPos = bounds.Y;

            if (ha == HorizontalAlignment.Center) xPos += bounds.Width / 2 - (int)textSize.X / 2;
            if (ha == HorizontalAlignment.Right) xPos += bounds.Width - (int)textSize.X;

            if (va == VerticalAlignment.Middle) yPos += bounds.Height / 2 - (int)textSize.Y / 2;
            if (va == VerticalAlignment.Bottom) yPos += bounds.Height - (int)textSize.Y;

            sb.DrawString(sf, text, new Vector2(xPos, yPos), clr);
        }

        public static void DrawAlignedText(SpriteBatch sb, BitmapFont sf, string text, Rectangle bounds, Color clr, HorizontalAlignment ha = HorizontalAlignment.Left, VerticalAlignment va = VerticalAlignment.Middle) {
            Vector2 textSize = sf.MeasureString(text);

            int xPos = bounds.X;
            int yPos = bounds.Y;

            if (ha == HorizontalAlignment.Center) xPos += bounds.Width / 2 - (int)textSize.X / 2;
            if (ha == HorizontalAlignment.Right) xPos += bounds.Width - (int)textSize.X;

            if (va == VerticalAlignment.Middle) yPos += bounds.Height / 2 - (int)textSize.Y / 2;
            if (va == VerticalAlignment.Bottom) yPos += bounds.Height - (int)textSize.Y;

            sb.DrawString(sf, text, new Vector2(xPos, yPos), clr);
        }

        /// <remarks> Source: https://stackoverflow.com/a/15987581/595437 </remarks>
        private static string WrapTextSegment(BitmapFont spriteFont, string text, float maxLineWidth) {
            string[] words      = text.Split(' ');
            var      sb         = new StringBuilder();
            float    lineWidth  = 0f;
            float    spaceWidth = spriteFont.MeasureString("  ").Width - spriteFont.MeasureString(" ").Width;
            float    aWidth     = spriteFont.MeasureString("a").Width;

            foreach (string word in words) {
                float wordWidth = spriteFont.MeasureString("a" + word).Width - aWidth;
                wordWidth = Math.Max(wordWidth, spriteFont.MeasureString(word + "a").Width - aWidth);

                if (lineWidth + wordWidth < maxLineWidth) {
                    sb.Append(word + " ");
                    lineWidth += wordWidth + spaceWidth;
                } else {
                    sb.Append("\n" + word + " ");
                    lineWidth = wordWidth + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth) {
            if (string.IsNullOrEmpty(text)) return "";

            return string.Join("\n", text.Split('\n').Select(s => WrapTextSegment(spriteFont, s, maxLineWidth)));
        }

    }
}
