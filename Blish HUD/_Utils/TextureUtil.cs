using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD {
    public static class TextureUtil {

        /// <summary>
        /// Creates a Texture2D from a stream, supports formats bmp, gif, jpg, png, tif and dds.
        /// The resulting texture has the alpha channel premultiplied to match the MonoGame 3.6
        /// implementation.
        /// </summary>
        /// <remarks>https://community.monogame.net/t/texture2d-fromstream-in-3-7/10973/9</remarks>
		public static Texture2D FromStreamPremultiplied(GraphicsDevice graphics, Stream stream) {
            var texture = Texture2D.FromStream(graphics, stream);

            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; ++i) {
                byte a = data[i].A;

                data[i].R = ApplyAlpha(data[i].R, a);
                data[i].G = ApplyAlpha(data[i].G, a);
                data[i].B = ApplyAlpha(data[i].B, a);
            }

            texture.SetData(data);

            return texture;
        }

        private static byte ApplyAlpha(byte color, byte alpha) {
            var fc = color / 255.0f;
            var fa = alpha / 255.0f;
            var fr = (int)(255.0f * fc * fa);
            if (fr < 0) {
                fr = 0;
            }
            if (fr > 255) {
                fr = 255;
            }
            return (byte)fr;
        }

    }
}
