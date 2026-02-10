using System;
using System.IO;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD {

    public class SpriteBatchParameters {

        public SpriteSortMode    SortMode          { get; set; }
        public BlendState        BlendState        { get; set; }
        public SamplerState      SamplerState      { get; set; }
        public DepthStencilState DepthStencilState { get; set; }
        public RasterizerState   RasterizerState   { get; set; }
        public Effect            Effect            { get; set; }
        public Matrix?           TransformMatrix   { get; set; }

        public SpriteBatchParameters(
            SpriteSortMode    sortMode          = SpriteSortMode.Deferred,
            BlendState        blendState        = null,
            SamplerState      samplerState      = null,
            DepthStencilState depthStencilState = null,
            RasterizerState   rasterizerState   = null,
            Effect            effect            = null,
            Matrix?           transformMatrix   = null
        ) {
            this.SortMode          = sortMode;
            this.BlendState        = blendState;
            this.SamplerState      = samplerState;
            this.DepthStencilState = depthStencilState;
            this.RasterizerState   = rasterizerState ?? BlishHud.Instance.UiRasterizer;
            this.Effect            = effect;
            this.TransformMatrix   = transformMatrix;
        }

        public static bool ParamsEqual(SpriteBatchParameters leftSpriteBatchParams, SpriteBatchParameters rightSpriteBatchParams) {
            return Equals(leftSpriteBatchParams, rightSpriteBatchParams)
                || (Equals(leftSpriteBatchParams.SortMode,          rightSpriteBatchParams.SortMode)
                 && Equals(leftSpriteBatchParams.BlendState,        rightSpriteBatchParams.BlendState)
                 && Equals(leftSpriteBatchParams.DepthStencilState, rightSpriteBatchParams.DepthStencilState)
                 && Equals(leftSpriteBatchParams.RasterizerState,   rightSpriteBatchParams.RasterizerState)
                 && Equals(leftSpriteBatchParams.Effect,            rightSpriteBatchParams.Effect)
                 && Equals(leftSpriteBatchParams.TransformMatrix,   rightSpriteBatchParams.TransformMatrix));
        }

    }

    public static class SpriteBatchExtensions {
        
        public static void Begin(this SpriteBatch spriteBatch, SpriteBatchParameters parameters) {
            spriteBatch.Begin(parameters.SortMode,
                              parameters.BlendState,
                              parameters.SamplerState,
                              parameters.DepthStencilState,
                              parameters.RasterizerState ?? BlishHud.Instance.UiRasterizer,
                              parameters.Effect,
                              parameters.TransformMatrix ?? GameService.Graphics.UIScaleTransform);
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Texture2D texture, Rectangle destinationRectangle) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             Color.White * ctrl.AbsoluteOpacity());
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Texture2D texture, Rectangle destinationRectangle, Color color) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             color * ctrl.AbsoluteOpacity());
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, TextureRegion2D texture, Rectangle destinationRectangle) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             Color.White * ctrl.AbsoluteOpacity());
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, TextureRegion2D texture, Rectangle destinationRectangle, Color color) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             color * ctrl.AbsoluteOpacity());
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             sourceRectangle,
                             Color.White * ctrl.AbsoluteOpacity());
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             sourceRectangle,
                             color * ctrl.AbsoluteOpacity());
        }

        public static void DrawOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None) {
            spriteBatch.Draw(texture,
                             destinationRectangle.ToBounds(ctrl.AbsoluteBounds),
                             sourceRectangle,
                             color * ctrl.AbsoluteOpacity(),
                             rotation,
                             origin,
                             effects,
                             0);
        }

        public static void DrawStringOnCtrl(this SpriteBatch    spriteBatch,
                                            Control             ctrl,
                                            string              text,
                                            BitmapFont          font,
                                            Rectangle           destinationRectangle,
                                            Color               color,
                                            bool                wrap                = false,
                                            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment   verticalAlignment   = VerticalAlignment.Middle) {
            DrawStringOnCtrl(spriteBatch,
                             ctrl,
                             text,
                             font,
                             destinationRectangle,
                             color,
                             wrap,
                             false,
                             1,
                             horizontalAlignment,
                             verticalAlignment,
                             clippingRectangle: null);
        }

        public static void DrawStringOnCtrl(this SpriteBatch    spriteBatch,
                                            Control             ctrl,
                                            string              text,
                                            BitmapFont          font,
                                            Rectangle           destinationRectangle,
                                            Color               color,
                                            bool                wrap,
                                            bool                stroke,
                                            int                 strokeDistance      = 1,
                                            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment   verticalAlignment   = VerticalAlignment.Middle) {
            DrawStringOnCtrl(spriteBatch,
                             ctrl,
                             text,
                             font,
                             destinationRectangle,
                             color,
                             wrap,
                             false,
                             1,
                             horizontalAlignment,
                             verticalAlignment,
                             clippingRectangle: null);
        }

        public static void DrawStringOnCtrl(this SpriteBatch    spriteBatch,
                                            Control             ctrl,
                                            string              text,
                                            BitmapFont          font,
                                            Rectangle           destinationRectangle,
                                            Color               color,
                                            bool                wrap,
                                            bool                stroke,
                                            int                 strokeDistance,
                                            HorizontalAlignment horizontalAlignment,
                                            VerticalAlignment   verticalAlignment,
                                            Rectangle?          clippingRectangle) {

            if (string.IsNullOrEmpty(text)) return;

            text = wrap ? DrawUtil.WrapText(font, text, destinationRectangle.Width) : text;

            // TODO: This does not account for vertical alignment
            if (horizontalAlignment != HorizontalAlignment.Left && (wrap || text.Contains("\n"))) {
                using (StringReader reader = new StringReader(text)) {
                    string line;

                    int lineHeightDiff = 0;

                    while (destinationRectangle.Height - lineHeightDiff > 0 && (line = reader.ReadLine()) != null) {
                        DrawStringOnCtrl(spriteBatch, ctrl, line, font, destinationRectangle.Add(0, lineHeightDiff, 0, -0), color, wrap, stroke, strokeDistance, horizontalAlignment, verticalAlignment, clippingRectangle);

                        lineHeightDiff += font.LineHeight;
                    }
                }

                return;
            }

            Vector2 textSize = font.MeasureString(text);

            clippingRectangle = clippingRectangle?.ToBounds(ctrl.AbsoluteBounds);

            destinationRectangle = destinationRectangle.ToBounds(ctrl.AbsoluteBounds);

            int xPos = destinationRectangle.X;
            int yPos = destinationRectangle.Y;

            switch (horizontalAlignment) {
                case HorizontalAlignment.Center:
                    xPos += destinationRectangle.Width / 2 - (int)textSize.X / 2;
                    break;
                case HorizontalAlignment.Right:
                    xPos += destinationRectangle.Width - (int)textSize.X;
                    break;
            }

            switch (verticalAlignment) {
                case VerticalAlignment.Middle:
                    yPos += destinationRectangle.Height / 2 - (int)textSize.Y / 2;
                    break;
                case VerticalAlignment.Bottom:
                    yPos += destinationRectangle.Height - (int)textSize.Y;
                    break;
            }

            var textPos = new Vector2(xPos, yPos);

            float absoluteOpacity = ctrl.AbsoluteOpacity();

            if (stroke) {
                var strokePreMultiplied = Color.Black * absoluteOpacity;

                spriteBatch.DrawString(font, text, textPos.OffsetBy(0,               -strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance,  -strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance,  0),               strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance,  strokeDistance),  strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(0,               strokeDistance),  strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, strokeDistance),  strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, 0),               strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, -strokeDistance), strokePreMultiplied, clippingRectangle);
            }

            spriteBatch.DrawString(font, text, textPos, color * absoluteOpacity, clippingRectangle);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth, Color color) {
            if (lineWidth <= 0 || bounds.Width <= 0 || bounds.Height <= 0) {
                return;
            }

            // Clamp lineWidth so it doesn't exceed half the rect size
            lineWidth = Math.Min(lineWidth, Math.Min(bounds.Width / 2, bounds.Height / 2));

            // Top
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
              new Rectangle(bounds.X, bounds.Y, bounds.Width, lineWidth), color);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
              new Rectangle(bounds.X, bounds.Bottom - lineWidth, bounds.Width, lineWidth), color);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
              new Rectangle(bounds.X, bounds.Y + lineWidth, lineWidth, bounds.Height - (lineWidth * 2)), color);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel,
              new Rectangle(bounds.Right - lineWidth, bounds.Y + lineWidth, lineWidth, bounds.Height - (lineWidth * 2)), color);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int lineWidth) {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, lineWidth, Color.Black);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, Color color) {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, Math.Min(bounds.Width, bounds.Height) / 2, color);
        }

        public static void DrawRectangleOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds) {
            DrawRectangleOnCtrl(spriteBatch, ctrl, bounds, Color.Black);
        }
    }
}
