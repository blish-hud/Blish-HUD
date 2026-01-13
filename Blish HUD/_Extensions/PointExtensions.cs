using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public enum ScaleMode {
        /// <summary>
        /// Scale to fit - Largest dimension fits within smallest dimension.
        /// </summary>
        Fit,
        /// <summary>
        /// Scale to fill - Largest dimension fits within largest dimension.
        /// </summary>
        Fill,
    }

    public static class PointExtensions {
        /// <summary>
        /// Converts a <see cref="Point"/> to a <see cref="System.Drawing.Point"/>
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to convert.</param>
        /// <returns>A new equivalent <see cref="System.Drawing.Point"/>.</returns>
        public static System.Drawing.Point ToSystemDrawingPoint(this Microsoft.Xna.Framework.Point point) {
            return new System.Drawing.Point(point.X, point.Y);
        }

        /// <summary>
        /// Scales a <see cref="Point"/> to the game UI scale.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to scale.</param>
        /// <returns>A new <see cref="Point"/> scaled to the game UI.</returns>
        public static Microsoft.Xna.Framework.Point ScaleToUi(this Microsoft.Xna.Framework.Point point) {
            return new Microsoft.Xna.Framework.Point((int)(point.X * GameService.Graphics.UIScaleMultiplier),
                                                     (int)(point.Y * GameService.Graphics.UIScaleMultiplier));
        }

        /// <summary>
        /// De-scales a <see cref="Point"/> from the game UI scale.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to de-scale.</param>
        /// <returns>A new <see cref="Point"/> without game UI scaling.</returns>
        public static Microsoft.Xna.Framework.Point UiToScale(this Microsoft.Xna.Framework.Point point) {
            return new Microsoft.Xna.Framework.Point((int)(point.X / GameService.Graphics.UIScaleMultiplier),
                                                     (int)(point.Y / GameService.Graphics.UIScaleMultiplier));
        }

        /// <summary>
        /// Creates a <see cref="Rectangle"/> of size <paramref name="point"/> within <paramref name="bounds"/>.
        /// </summary>
        /// <param name="point">The size of the new <see cref="Rectangle"/>.</param>
        /// <param name="bounds">The parent <see cref="Rectangle"/></param>
        /// <returns>A new <see cref="Rectangle"/> of size <paramref name="point"/> within <paramref name="bounds"/></returns>
        public static Rectangle InBounds(this Point point, Rectangle bounds) {
            return new Rectangle(bounds.Location, point);
        }

        /// <summary>
        /// Converts a <see cref="Point"/> to a <see cref="System.Drawing.Size"/>
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to convert.</param>
        /// <returns>A new equvalent <see cref="System.Drawing.Point"/>.</returns>
        public static System.Drawing.Size ToSystemDrawingSize(this Microsoft.Xna.Framework.Point point) {
            return new System.Drawing.Size(point.X, point.Y);
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Point"/> to a <see cref="Point"/>
        /// </summary>
        /// <param name="point">The <see cref="System.Drawing.Point"/> to convert.</param>
        /// <returns>A new equvalent <see cref="Point"/>.</returns>
        public static Microsoft.Xna.Framework.Point ToXnaPoint(this System.Drawing.Point point) {
            return new Microsoft.Xna.Framework.Point(point.X, point.Y);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <param name="enlarge">A value indicating whether to allow scale ratios above 1.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="maxWidth"/>, <paramref name="maxHeight"/></returns>
        public static Point ResizeKeepAspect(this Point src, int maxWidth, int maxHeight, ScaleMode scaleMode, bool enlarge) {
            float scale = GetAspectRatioScale(src, maxWidth, maxHeight, scaleMode, enlarge);
            return new Point((int)Math.Round(src.X * scale), (int)Math.Round(src.Y * scale));
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The point to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <param name="enlarge">A value indicating whether to allow scale ratios above 1.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="maxWidth"/>, <paramref name="maxHeight"/></returns>
        public static Point ResizeKeepAspect(this Point src, int maxWidth, int maxHeight, bool enlarge)
        {
            return ResizeKeepAspect(src, maxWidth, maxHeight, ScaleMode.Fit, enlarge);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="maxWidth"/>, <paramref name="maxHeight"/></returns>
        public static Point ResizeKeepAspect(this Point src, int maxWidth, int maxHeight, ScaleMode scaleMode) {
            return ResizeKeepAspect(src, maxWidth, maxHeight, scaleMode, false);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="maxWidth"/>, <paramref name="maxHeight"/></returns>
        public static Point ResizeKeepAspect(this Point src, int maxWidth, int maxHeight) {
            return ResizeKeepAspect(src, maxWidth, maxHeight, ScaleMode.Fit, false);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <param name="enlarge">A value indicating whether to allow scale ratios above 1.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="max"/></returns>
        public static Point ResizeKeepAspect(this Point src, Point max, ScaleMode scaleMode, bool enlarge) {
            return ResizeKeepAspect(src, max.X, max.Y, scaleMode, enlarge);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <param name="enlarge">A value indicating whether to allow scale ratios above 1.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="max"/></returns>
        public static Point ResizeKeepAspect(this Point src, Point max, bool enlarge) {
            return ResizeKeepAspect(src, max.X, max.Y, ScaleMode.Fit, enlarge);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="max"/></returns>
        public static Point ResizeKeepAspect(this Point src, Point max, ScaleMode scaleMode) {
            return ResizeKeepAspect(src, max.X, max.Y, scaleMode, false);
        }

        /// <summary>
        /// Resize a <see cref="Point"/> to fit within another <see cref="Point"/>, maintaining aspect ratio.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <returns>A new <see cref="Point"/> with the same aspect ratio as <paramref name="src"/>, scaled to <paramref name="max"/></returns>
        public static Point ResizeKeepAspect(this Point src, Point max) {
            return ResizeKeepAspect(src, max.X, max.Y, ScaleMode.Fit, false);
        }

        /// <summary>
        /// Get the minimum locked aspect ratio scale factor between two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <param name="enlarge">A value indicating whether to allow scale ratios above 1.</param>
        /// <returns>A scale value for scaling <paramref name="src"/> with a locked aspect ratio to <paramref name="maxHeight"/>, <paramref name="maxWidth"/></returns>
        public static float GetAspectRatioScale(this Point src, int maxWidth, int maxHeight, ScaleMode scaleMode, bool enlarge) {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.X);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Y);

            float xScale = maxWidth / (float)src.X;
            float yScale = maxHeight / (float)src.Y;

            return scaleMode switch {
                ScaleMode.Fit => Math.Min(xScale, yScale),
                ScaleMode.Fill => Math.Max(xScale, yScale),
                _ => throw new NotImplementedException($"Unknown scaleMode {scaleMode}")
            };
        }

        /// <summary>
        /// Get the minimum locked aspect ratio scale factor between two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <returns>A scale value for scaling <paramref name="src"/> with a locked aspect ratio to <paramref name="maxHeight"/>, <paramref name="maxWidth"/></returns>
        public static float GetAspectRatioScale(this Point src, int maxWidth, int maxHeight, ScaleMode scaleMode) {
            return GetAspectRatioScale(src, maxWidth, maxHeight, scaleMode, false);
        }

        /// <summary>
        /// Get the minimum locked aspect ratio scale factor between two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="maxWidth">The maximum X bound to scale within.</param>
        /// <param name="maxHeight">The maximum Y bound to scale within.</param>
        /// <returns>A scale value for scaling <paramref name="src"/> with a locked aspect ratio to <paramref name="maxHeight"/>, <paramref name="maxWidth"/></returns>
        public static float GetAspectRatioScale(this Point src, int maxWidth, int maxHeight) {
            return GetAspectRatioScale(src, maxWidth, maxHeight, ScaleMode.Fit, false);
        }

        /// <summary>
        /// Get the minimum locked aspect ratio scale factor between two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <param name="enlarge">A value indicating whether to allow scale ratios above 1.</param>
        /// <returns></returns>
        public static float GetAspectRatioScale(this Point src, Point max, ScaleMode scaleMode, bool enlarge) {
            return GetAspectRatioScale(src, max.X, max.Y, scaleMode, enlarge);
        }

        /// <summary>
        /// Get the minimum locked aspect ratio scale factor between two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <param name="scaleMode">The scaling mode to use when calculating the ratio.</param>
        /// <returns>A scale value for scaling <paramref name="src"/> with a locked aspect ratio to <paramref name="max"/></returns>
        public static float GetAspectRatioScale(this Point src, Point max, ScaleMode scaleMode) {
            return GetAspectRatioScale(src, max.X, max.Y, scaleMode, false);
        }

        /// <summary>
        /// Get the minimum locked aspect ratio scale factor between two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="src">The <see cref="Point"/> to scale.</param>
        /// <param name="max">The maximum bounds to scale within.</param>
        /// <returns>A scale value for scaling <paramref name="src"/> with a locked aspect ratio to <paramref name="max"/></returns>
        public static float GetAspectRatioScale(this Point src, Point max) {
            return GetAspectRatioScale(src, max.X, max.Y, ScaleMode.Fit, false);
        }
    }
}
