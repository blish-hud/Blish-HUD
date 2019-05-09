using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Utils;
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
            this.RasterizerState   = rasterizerState ?? Overlay._uiRasterizer;
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
                              parameters.RasterizerState ?? Overlay._uiRasterizer,
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

        public static void DrawStringOnCtrl(
            this SpriteBatch             spriteBatch,
            Control                      ctrl,
            string                       text,
            BitmapFont                   font,
            Rectangle                    destinationRectangle,
            Color                        color,
            bool                         wrap                = false,
            DrawUtil.HorizontalAlignment horizontalAlignment = DrawUtil.HorizontalAlignment.Left,
            DrawUtil.VerticalAlignment   verticalAlignment   = DrawUtil.VerticalAlignment.Middle
        ) {
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
                             verticalAlignment);
        }

        public static void DrawStringOnCtrl(this SpriteBatch spriteBatch,
                                            Control ctrl,
                                            string text,
                                            BitmapFont font,
                                            Rectangle destinationRectangle,
                                            Color color,
                                            bool wrap,
                                            bool stroke,
                                            int strokeDistance = 1,
                                            DrawUtil.HorizontalAlignment horizontalAlignment = DrawUtil.HorizontalAlignment.Left,
                                            DrawUtil.VerticalAlignment verticalAlignment = DrawUtil.VerticalAlignment.Middle) {

            if (string.IsNullOrWhiteSpace(text)) return;

            text = wrap ? DrawUtil.WrapText(font, text, destinationRectangle.Width) : text;

            // TODO: This does not account for vertical alignment
            if (horizontalAlignment != DrawUtil.HorizontalAlignment.Left && (wrap || text.Contains("\n"))) {
                using (StringReader reader = new StringReader(text)) {
                    string line;

                    int lineHeightDiff = 0;

                    while (destinationRectangle.Height - lineHeightDiff > 0 && (line = reader.ReadLine()) != null) {
                        DrawStringOnCtrl(spriteBatch, ctrl, line, font, destinationRectangle.Add(0, lineHeightDiff, 0, -0), color, wrap, stroke, strokeDistance, horizontalAlignment, verticalAlignment);

                        lineHeightDiff += font.LineHeight;
                    }
                }

                return;
            }

            Vector2 textSize = font.MeasureString(text);

            destinationRectangle = destinationRectangle.ToBounds(ctrl.AbsoluteBounds);

            int xPos = destinationRectangle.X;
            int yPos = destinationRectangle.Y;

            switch (horizontalAlignment) {
                case DrawUtil.HorizontalAlignment.Center:
                    xPos += destinationRectangle.Width / 2 - (int)textSize.X / 2;
                    break;
                case DrawUtil.HorizontalAlignment.Right:
                    xPos += destinationRectangle.Width - (int)textSize.X;
                    break;
            }

            switch (verticalAlignment) {
                case DrawUtil.VerticalAlignment.Middle:
                    yPos += destinationRectangle.Height / 2 - (int)textSize.Y / 2;
                    break;
                case DrawUtil.VerticalAlignment.Bottom:
                    yPos += destinationRectangle.Height - (int)textSize.Y;
                    break;
            }

            var textPos = new Vector2(xPos, yPos);

            if (stroke) {
                spriteBatch.DrawString(font, text, textPos.OffsetBy(0, -strokeDistance), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance, -strokeDistance), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance, 0), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance, strokeDistance), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(0, strokeDistance), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, strokeDistance), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, 0), Color.Black * ctrl.AbsoluteOpacity());
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, -strokeDistance), Color.Black * ctrl.AbsoluteOpacity());
            }

            spriteBatch.DrawString(font, text, textPos, color * ctrl.AbsoluteOpacity());
        }

    }
}
