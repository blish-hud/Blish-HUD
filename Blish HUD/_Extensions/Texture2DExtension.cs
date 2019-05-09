using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {
    public static class Texture2DExtension {

        /// <remarks>https://stackoverflow.com/a/16141281/595437</remarks>
        public static Texture2D GetRegion(this Texture2D texture2D, Rectangle region) {
            Texture2D croppedTexture = new Texture2D(GameService.Graphics.GraphicsDevice, region.Width, region.Height);

            Color[] clrData = new Color[region.Width * region.Height];
            texture2D.GetData(0, region, clrData, 0, region.Width * region.Height);
            croppedTexture.SetData(clrData);

            return croppedTexture;
        }

        public static Texture2D GetRegion(this Texture2D texture2D, int x, int y, int width, int height) {
            return GetRegion(texture2D, new Rectangle(x, y, width, height));
        }

        public static Texture2D Duplicate(this Texture2D texture2D) {
            return GetRegion(texture2D, texture2D.Bounds);
        }

        public static Texture2D SetRegion(this Texture2D texture2D, Rectangle region, Color color) {
            if (texture2D == null)
                throw new ArgumentNullException(nameof(texture2D));
            if (region.X < 0)
                throw new ArgumentOutOfRangeException(nameof(region.X));
            if (region.Y < 0)
                throw new ArgumentOutOfRangeException(nameof(region.Y));
            if (region.Right > texture2D.Bounds.Right)
                throw new ArgumentOutOfRangeException(nameof(region.Right));
            if (region.Bottom > texture2D.Bounds.Bottom)
                throw new ArgumentOutOfRangeException(nameof(region.Bottom));

            Color[] colorData = new Color[region.Width * region.Height];

            for (int i = 0; i < colorData.Length - 1; i++) {
                colorData[i] = color;
            }

            texture2D.SetData(0, region, colorData, 0, colorData.Length);

            return texture2D;
        }

        public static Texture2D SetRegion(this Texture2D texture2D, int x, int y, int width, int height, Color color) {
            return SetRegion(texture2D, new Rectangle(x, y, width, height), color);
        }

    }
}
