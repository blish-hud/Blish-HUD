using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Annotations;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities {
    public class ScrollingTrailSection : Trail, ITrail {

        #region Static Effect Loading

        private static Effect _basicTrailEffect;

        static ScrollingTrailSection() {
            _basicTrailEffect = Overlay.cm.Load<Effect>("effects\\trail");
        }

        #endregion

        private float _animationSpeed = 1;

        private float _fadeNear = 10000;
        private float _fadeFar  = 10000;
        private float _scale    = 1;

        private VertexPositionColorTexture[] VertexData { get; set; }

        public float AnimationSpeed {
            get => _animationSpeed;
            set => SetProperty(ref _animationSpeed, value);
        }

        public override Texture2D TrailTexture {
            get => _trailTexture;
            set {
                if (SetProperty(ref _trailTexture, value))
                    OnTrailPointsChanged();
            }
        }

        public float FadeNear {
            get => _fadeNear;
            set => SetProperty(ref _fadeNear, value);
        }

        public float FadeFar {
            get => _fadeFar;
            set => SetProperty(ref _fadeFar, value);
        }

        public float Scale {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }


        public ScrollingTrailSection() : base(null) { /* NOOP */ }

        public ScrollingTrailSection([CanBeNull] List<Vector3> trailPoints) : base(trailPoints) { /* NOOP */ }

        public override void OnTrailPointsChanged() {
            if (!_trailPoints.Any()) return;

            //SmoothTrail(ref _trailPoints);

            this.VertexData = new VertexPositionColorTexture[this.TrailPoints.Count * 2];

            float imgScale = ScrollingTrail.TRAIL_WIDTH;

            float pastDistance = this.TrailLength;

            var offsetDirection = new Vector3(0, 0, -1);

            var currPoint = this.TrailPoints[0];
            Vector3 offset = Vector3.Zero;

            for (int i = 0; i < this.TrailPoints.Count - 1; i++) {
                var nextPoint = this.TrailPoints[i + 1];

                var pathDirection = nextPoint - currPoint;

                pathDirection.Normalize();

                offset = Vector3.Cross(pathDirection, offsetDirection);

                var leftPoint = currPoint + (offset * imgScale);
                var rightPoint = currPoint + (offset * -imgScale);

                this.VertexData[i * 2 + 1] = new VertexPositionColorTexture(leftPoint, Color.White, new Vector2(0f, pastDistance / (imgScale * 2) - 1));
                this.VertexData[i * 2] = new VertexPositionColorTexture(rightPoint, Color.White, new Vector2(1f, pastDistance / (imgScale * 2) - 1));

                pastDistance -= Vector3.Distance(currPoint, nextPoint);

                currPoint = nextPoint;

                #if DEBUG
                //var leftBoxPoint = new Cube() {
                //    Color    = Color.Red,
                //    Size     = new Vector3(0.25f),
                //    Position = leftPoint
                //};
                //var rightBoxPoint = new Cube() {
                //    Color    = Color.Red,
                //    Size     = new Vector3(0.25f),
                //    Position = rightPoint
                //};
                //GameService.Graphics.World.Entities.Add(leftBoxPoint);
                //GameService.Graphics.World.Entities.Add(rightBoxPoint);
                #endif
            }

            var fleftPoint  = currPoint + (offset * imgScale);
            var frightPoint = currPoint + (offset * -imgScale);

            this.VertexData[this.TrailPoints.Count * 2 - 1] = new VertexPositionColorTexture(fleftPoint,  Color.White, new Vector2(0f, pastDistance / (imgScale * 2) - 1));
            this.VertexData[this.TrailPoints.Count * 2 - 2] = new VertexPositionColorTexture(frightPoint, Color.White, new Vector2(1f, pastDistance / (imgScale * 2) - 1));
        }

        private void SmoothTrail(ref List<Vector3> pointList) {
            List<Vector3> smoothedPoints = new List<Vector3>();

            //for (int i = 1; i < pointList.Count; i++) {
            //    if (Vector3.Distance(pointList[i - 1], pointList[i]) < 30f) {
            //        pointList.RemoveAt(i);
            //        i--;
            //    }
            //}

            if (pointList.Count < 4) return;

            smoothedPoints.Add(pointList[0]);

            for (int i = 1; i < pointList.Count - 2; i++) {
                smoothedPoints.Add(pointList[i]);

                smoothedPoints.Add(Vector3.CatmullRom(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2], .9f));
                //smoothedPoints.Add(Vector2.CatmullRom(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2], .2f));
                //smoothedPoints.Add(Vector2.CatmullRom(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2], .3f));
                //smoothedPoints.Add(Vector2.CatmullRom(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2], .7f));
                //smoothedPoints.Add(Vector2.CatmullRom(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2], .8f));
                //smoothedPoints.Add(Vector2.CatmullRom(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2], .9f));
            }

            smoothedPoints.Add(pointList[pointList.Count - 2]);
            smoothedPoints.Add(pointList[pointList.Count - 1]);

            pointList.Clear();
            pointList.AddRange(smoothedPoints);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            _basicTrailEffect.Parameters["TotalMilliseconds"].SetValue((float)gameTime.TotalGameTime.TotalMilliseconds);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            if (this.TrailTexture == null || this.VertexData == null || this.VertexData.Length < 3) return;

            _basicTrailEffect.Parameters["WorldViewProjection"].SetValue(GameService.Camera.WorldViewProjection);
            _basicTrailEffect.Parameters["PlayerViewProjection"].SetValue(GameService.Camera.PlayerView * GameService.Camera.Projection);
            _basicTrailEffect.Parameters["Texture"].SetValue(this.TrailTexture);
            _basicTrailEffect.Parameters["FlowSpeed"].SetValue(this.AnimationSpeed);
            _basicTrailEffect.Parameters["PlayerPosition"].SetValue(GameService.Player.Position);
            _basicTrailEffect.Parameters["FadeNear"].SetValue(this.FadeNear);
            _basicTrailEffect.Parameters["FadeFar"].SetValue(this.FadeFar);
            _basicTrailEffect.Parameters["Opacity"].SetValue(this.Opacity);
            _basicTrailEffect.Parameters["TotalLength"].SetValue(20f); // this.TrailLength / this.TrailTexture.Height * 2);

            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (EffectPass trailPass in _basicTrailEffect.CurrentTechnique.Passes) {
                trailPass.Apply();

                ((BasicEffect)this.EntityEffect).VertexColorEnabled = true;

                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip,
                                                  this.VertexData,
                                                  0,
                                                  this.VertexData.Length - 2);
            }
        }

    }
}
