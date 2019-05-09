using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Utils {
    public static class DrawUtil {

        public enum HorizontalAlignment {
            Left,
            Center,
            Right
        }

        public enum VerticalAlignment {
            Top,
            Middle,
            Bottom
        }

        public enum TextOverflow {
            Ellipsis,
            Wrap,
            Clip
        }

        //public static Vector3 Gw2V3toMonogameV3(GW2NET.Common.Drawing.Vector3D gw2v3) {
        //    return new Vector3((float)gw2v3.X, (float)gw2v3.Z, (float)gw2v3.Y);
        //}

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
        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth) {
            string[] words      = text.Split(' ');
            var      sb         = new StringBuilder();
            float    lineWidth  = 0f;
            float    spaceWidth = spriteFont.MeasureString(" ").Width;

            foreach (string word in words) {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth) {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                } else {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public static Quaternion LookAt(Vector3 forwardVector) {
            float dot = Vector3.Dot(Vector3.Forward, forwardVector);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f) {
                return new Quaternion(Vector3.Up.X, Vector3.Up.Y, Vector3.Up.Z, (float)Math.PI);
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f) {
                return Quaternion.Identity;
            }

            float rotAngle = (float)Math.Acos(dot);
            var rotAxis = Vector3.Cross(Vector3.Forward, forwardVector);
            rotAxis = Vector3.Normalize(rotAxis);

            return CreateFromAxisAngle(rotAxis, rotAngle);
        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle) {
            float halfAngle = angle * .5f;
            float s = (float)Math.Sin(halfAngle);
            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, (float)Math.Cos(halfAngle));
        }

        public static Vector3 UpVectorFromCameraForward(Vector3 camForward) {
            var CameraRight = Vector3.Cross(camForward, Vector3.Backward);
            var CameraUp = Vector3.Cross(CameraRight, camForward);
            CameraUp.Normalize();

            return CameraUp;
        }

        public static VertexPositionTexture[] CreateFlatSquare(Vector3 pos, Vector2 size) {
            var squareVerts = new VertexPositionTexture[4];

            //Vector3 center = new Vector3(pos.X - size.X / 2, pos.Y - size.Y / 2, pos.Z);

            squareVerts[0] = new VertexPositionTexture(pos + new Vector3(size.X / 2, size.Y / 2, 0), new Vector2(1, 1));
            squareVerts[1] = new VertexPositionTexture(pos + new Vector3(size.X / 2, size.Y / -2, 0), new Vector2(1, 0));
            squareVerts[2] = new VertexPositionTexture(pos + new Vector3(size.X / -2, size.Y / 2, 0), new Vector2(0, 1));
            squareVerts[3] = new VertexPositionTexture(pos + new Vector3(size.X / -2, size.Y / -2, 0), new Vector2(0, 0));

            return squareVerts;
        }

        public static Texture2D DrawCircle(GraphicsDevice graphicsDevice, int radius, int borderThickness) {
            int diam = radius * 2;
            int radsq = radius * radius;
            float insideradsq = (float)Math.Pow(radius - borderThickness, 2);

            var texture = new Texture2D(graphicsDevice, diam, diam);
            var colorData = new Color[diam * diam];

            for (int x = 0; x < diam; x++) {
                for (int y=0; y < diam; y++) {
                    int index = x * diam + y;
                    var pos = new Vector2(x - radius, y - radius);
                    float circLength = pos.LengthSquared();
                    if (circLength <= radsq && circLength >= insideradsq) {
                        colorData[index] = Color.White;
                    } else {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            

            texture.SetData(colorData);
            return texture;
        }

    }
}
