using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Entities.Effects;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities {
    public class ScrollingTrailSection : Trail, ITrail {

        #region Load Static

        private static readonly TrailEffect _sharedTrailEffect;

        static ScrollingTrailSection() {
            _sharedTrailEffect = new TrailEffect(GameService.Content.ContentManager.Load<Effect>("effects\\trail"));
        }

        #endregion

        private float _animationSpeed = 1;
        private float _fadeNear       = 10000;
        private float _fadeFar        = 10000;
        private float _scale          = 1;
        private Color _tintColor      = Color.White;

        private VertexPositionColorTexture[] VertexData { get; set; }

        private VertexBuffer _vertexBuffer;

        public float AnimationSpeed {
            get => _animationSpeed;
            set => SetProperty(ref _animationSpeed, value);
        }

        public override Texture2D TrailTexture {
            get => _trailTexture;
            set {
                if (SetProperty(ref _trailTexture, value))
                    InitTrailPoints();
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

        public Color TintColor {
            get => _tintColor;
            set => SetProperty(ref _tintColor, value);
        }
        
        public ScrollingTrailSection() : base(null) { /* NOOP */ }

        public ScrollingTrailSection(List<Vector3> trailPoints) : base(trailPoints) { /* NOOP */ }

        private List<Vector3> SetTrailResolution(List<Vector3> trailPoints, float pointResolution) {
            List<Vector3> tempTrail = new List<Vector3>();

            var lstPoint = trailPoints[0];

            for (int i = 0; i < trailPoints.Count; i++) {
                var dist = Vector3.Distance(lstPoint, trailPoints[i]);

                var s   = dist / pointResolution;
                var inc = 1    / s;

                for (float v = inc; v < s - inc; v += inc) {
                    var nPoint = Vector3.Lerp(lstPoint, _trailPoints[i], v / s);

                    tempTrail.Add(nPoint);
                }

                tempTrail.Add(trailPoints[i]);

                lstPoint = trailPoints[i];
            }

            return tempTrail;
        }

        protected override void InitTrailPoints() {
            if (!_trailPoints.Any()) return;

            // TacO has a minimum of 30, so we'll use 30
            _trailPoints = SetTrailResolution(_trailPoints, 30f);

            this.VertexData = new VertexPositionColorTexture[this.TrailPoints.Count * 2];

            float imgScale = ScrollingTrail.TRAIL_WIDTH;

            float pastDistance = this.TrailLength;

            var offsetDirection = new Vector3(0, 0, -1);

            var currPoint = this.TrailPoints[0];
            Vector3 offset = Vector3.Zero;

            for (int i = 0; i < this.TrailPoints.Count - 1; i++) {
                var nextPoint = this.TrailPoints[i + 1];

                var pathDirection = nextPoint - currPoint;

                offset = Vector3.Cross(pathDirection, offsetDirection);

                offset.Normalize();

                var leftPoint = currPoint + (offset * imgScale);
                var rightPoint = currPoint + (offset * -imgScale);

                this.VertexData[i * 2 + 1] = new VertexPositionColorTexture(leftPoint, Color.White, new Vector2(0f, pastDistance / (imgScale * 2) - 1));
                this.VertexData[i * 2] = new VertexPositionColorTexture(rightPoint, Color.White, new Vector2(1f, pastDistance / (imgScale * 2) - 1));

                pastDistance -= Vector3.Distance(currPoint, nextPoint);

                currPoint = nextPoint;

#if PLOTTRAILS
                GameService.Overlay.QueueMainThreadUpdate((gameTime) => {
                    var leftBoxPoint = new Cube() {
                        Color    = Color.Red,
                        Size     = new Vector3(0.25f),
                        Position = leftPoint
                    };

                    var rightBoxPoint = new Cube() {
                        Color = Color.Red,
                        Size = new Vector3(0.25f),
                        Position = rightPoint
                    };

                    GameService.Graphics.World.Entities.Add(leftBoxPoint);
                    GameService.Graphics.World.Entities.Add(rightBoxPoint);
                });
#endif
            }

            var fleftPoint  = currPoint + (offset * imgScale);
            var frightPoint = currPoint + (offset * -imgScale);

            this.VertexData[this.TrailPoints.Count * 2 - 1] = new VertexPositionColorTexture(fleftPoint,  Color.White, new Vector2(0f, pastDistance / (imgScale * 2) - 1));
            this.VertexData[this.TrailPoints.Count * 2 - 2] = new VertexPositionColorTexture(frightPoint, Color.White, new Vector2(1f, pastDistance / (imgScale * 2) - 1));

            _vertexBuffer = new VertexBuffer(BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, this.VertexData.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(this.VertexData);
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            if (this.TrailTexture == null || this.VertexData == null || this.VertexData.Length < 3) return;

            _sharedTrailEffect.SetEntityState(_trailTexture,
                                              _animationSpeed,
                                              _fadeNear,
                                              _fadeFar,
                                              _opacity,
                                              20f,
                                              _tintColor);

            graphicsDevice.SetVertexBuffer(_vertexBuffer, 0);

            foreach (EffectPass trailPass in _sharedTrailEffect.CurrentTechnique.Passes) {
                trailPass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vertexBuffer.VertexCount - 2);
            }
        }

    }
}
