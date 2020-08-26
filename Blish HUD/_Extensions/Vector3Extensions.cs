using System;
using System.Collections.Generic;
using System.Linq;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public static class Vector3Extensions {

        /// <summary>
        /// Converts Gw2Sharp's left-handed <see cref="Coordinates3"/> to XNA's right-handed <see cref="Vector3"/>.
        /// </summary>
        public static Vector3 ToXnaVector3(this Coordinates3 vector) {
            return new Vector3((float)vector.X, (float)vector.Z, (float)vector.Y);
        }

        public static string ToRoundedString(this Vector3 vector) {
            return $"X: {vector.X:0,0} Y: {vector.Y:0,0} Z: {vector.Z:0,0}";
        }

        /// <summary>
        /// Creates a list of points sampled semi-equidistant along the Cubic-Hermite interpolated curve from a list of points.
        /// </summary>
        /// <param name="points">The list of points the curve is generated from.</param>
        /// <param name="resolution">Distance between each sampled point</param>
        /// <param name="tension">Length of the tangents. 0 gives no overshoot, 1 gives a lot of overshoot.</param>
        /// <param name="smartSampling">Whether or not the curve should sample based on the curvature of the curve at each sampling point.</param>
        /// <param name="curvatureLowerBound">If <paramref name="smartSampling"/> is true, only sample points with a higher curvature than this parameter.</param>
        /// <param name="curvatureUpperBound">If <paramref name="smartSampling"/>  is true, sample <paramref name="upsampleCount"/> points between points with a curvature higher than this parameter.</param>
        /// <param name="upsampleCount">If <paramref name="smartSampling"/>  is true, the amount of points to upsample, between points with a curvature higher than <paramref name="curvatureUpperBound"/>.</param>
        public static List<Vector3> CreateHermiteCurve(this List<Vector3> points, float resolution = 0.15f, float tension = 0.5f,
                                                       bool smartSampling = true, float curvatureLowerBound = 0.05f,
                                                       float curvatureUpperBound = 2f, uint upsampleCount = 10) {
            List<Vector3> hermitePoints = new List<Vector3>();

            tension = MathHelper.Clamp(tension, 0f, 1.0f);

            //Hermite basis functions
            Func<float, float> h00 = t => (1 + 2 * t) * (float)Math.Pow(1 - t, 2.0f);
            Func<float, float> h10 = t => t * (float)Math.Pow(1 - t, 2.0f);
            Func<float, float> h01 = t => (float)Math.Pow(t, 2.0f) * (3 - 2 * t);
            Func<float, float> h11 = t => (float)Math.Pow(t, 2.0f) * (t - 1);

            Vector3 p0, p1, m0, m1;

            float SplineLength() {

                Vector3 c0 = m0;
                Vector3 c1 = 6f * (p1 - p0) - 4f * m0 - 2f * m1;
                Vector3 c2 = 6f * (p0 - p1) + 3f * (m1 + m0);

                Func<float, Vector3> derivative = t => c0 + t * (c1 + t * c2);

                List<Vector2> GaussLegendreCoefficients = new List<Vector2>() {
                  new Vector2(0.0f, 0.5688889f),
                  new Vector2( -0.5384693f, 0.47862867f ),
                  new Vector2(0.5384693f, 0.47862867f ),
                  new Vector2( -0.90617985f, 0.23692688f ),
                  new Vector2( 0.90617985f, 0.23692688f )
                };

                float length = 0.0f;

                foreach (var coeff in GaussLegendreCoefficients) {
                    float t = 0.5f * (1.0f + coeff.X);
                    length += derivative(t).Length() * coeff.Y;
                }
                return 0.5f * length;

            }

            float GetCurvature(float t0) {
                //First derivative
                Func<float, float> h00dt = t => 6 * t * t - 6 * t;
                Func<float, float> h10dt = t => 3 * t * t - 4 * t + 1;
                Func<float, float> h01dt = t => -6 * t * t + 6 * t;
                Func<float, float> h11dt = t => 3 * t * t - 2 * t;

                //Second derivative
                Func<float, float> h00dt2 = t => 12 * t - 6;
                Func<float, float> h10dt2 = t => 6 * t - 4;
                Func<float, float> h01dt2 = t => -12 * t + 6;
                Func<float, float> h11dt2 = t => 6 * t - 2;

                var curvature = (float)(Vector3.Cross(h00dt(t0) * p0 + h10dt(t0) * m0 + h01dt(t0) * p1 + h11dt(t0) * m1,
                                              h00dt2(t0) * p0 + h10dt2(t0) * m0 + h01dt2(t0) * p1 + h11dt2(t0) * m1).Length()
                                              / Math.Pow((h00dt(t0) * p0 + h10dt(t0) * m0 + h01dt(t0) * p1 + h11dt(t0) * m1).Length(), 3));
                return curvature;
            }

            hermitePoints.Add(points.First());

            var prevPoint = points.First();

            for (int k = 0; k < points.Count - 1; k++) {

                p0 = points[k];
                p1 = points[k + 1];

                if (k > 0)
                    m0 = tension * (p1 - points[k - 1]);
                else
                    m0 = p1 - p0;

                if (k < points.Count - 2)
                    m1 = tension * (points[k + 2] - p0);
                else
                    m1 = p1 - p0;

                var numPoints = (uint) (SplineLength() / resolution);
                var kappa = 0.0f;

                for (int i = 0; i < numPoints; i++) {
                    var t = i * (1.0f / numPoints);

                    if (smartSampling)
                        kappa = GetCurvature(t);

                    var sampledPoint = h00(t) * p0 + h10(t) * m0 + h01(t) * p1 + h11(t) * m1;

                    if (smartSampling && kappa < curvatureLowerBound && (prevPoint - sampledPoint).Length() < 10) continue;

                    prevPoint = sampledPoint;
                    hermitePoints.Add(sampledPoint);

                    if (smartSampling && kappa > curvatureUpperBound) {
                        var t1 = (i + 1) * (1.0f / numPoints);
                        var delta = 1.0f / upsampleCount;

                        for (float n = delta; n < 1; n += delta) {
                            var dt = (t1 - t) * n;
                            hermitePoints.Add(h00(t + dt) * p0 + h10(t + dt) * m0 + h01(t + dt) * p1 + h11(t + dt) * m1);
                        }
                    }
                }
            }
            hermitePoints.Add(points.Last());
            return hermitePoints;
        }
    }
}
