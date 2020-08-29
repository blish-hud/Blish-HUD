using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Pathing.Entities.Effects;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities {
    public class ScrollingTrailSection : Trail, ITrail {

        #region Load Static

        private static readonly TrailEffect _sharedTrailEffect;
        private static readonly Texture2D _fadeTexture;

        static ScrollingTrailSection() {
            _sharedTrailEffect = new TrailEffect(GameService.Content.ContentManager.Load<Effect>("effects\\trail"));
            _fadeTexture = GameService.Content.GetTexture("uniformclouds_blur30");
        }

        #endregion

        private float _animationSpeed   = 1;
        private float _fadeNear         = 10000;
        private float _fadeFar          = 10000;
        private float _scale            = 1;
        private float _playerFadeRadius = 0.25f;
        private bool _fadeCenter        = true;
        private Color _tintColor        = Color.White;

        private VertexPositionColorTexture[] VertexData { get; set; }

        private VertexBuffer _vertexBuffer;
        private List<Func<List<Vector3>, List<Vector3>>> _postProcessFunctions;

        public override Texture2D TrailTexture {
            get => _trailTexture;
            set => SetProperty(ref _trailTexture, value);
        }

        public float AnimationSpeed {
            get => _animationSpeed;
            set => SetProperty(ref _animationSpeed, value);
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

        public float PlayerFadeRadius {
            get => _playerFadeRadius;
            set => SetProperty(ref _playerFadeRadius, value);
        }

        public bool FadeCenter {
            get => _fadeCenter;
            set => SetProperty(ref _fadeCenter, value);
        }

        public Color TintColor {
            get => _tintColor;
            set => SetProperty(ref _tintColor, value);
        }

        public List<Func<List<Vector3>, List<Vector3>>> PostProcessFunctions {
            get => _postProcessFunctions ??= new List<Func<List<Vector3>, List<Vector3>>>();
            set {
                if (SetProperty(ref _postProcessFunctions, value))
                    InitTrailPoints();
            }
        }

        public ScrollingTrailSection() : base(null) { /* NOOP */ }

        public ScrollingTrailSection(List<Vector3> trailPoints) : base(trailPoints) { /* NOOP */ }


        protected override List<Vector3> PostProcess() {
            var tempPoints = _trailPoints;
            foreach(var k in PostProcessFunctions) {
                tempPoints = k.Invoke(tempPoints);
            }
            return tempPoints;
        }

        protected override void InitTrailPoints() {
            if (!_trailPoints.Any()) return;
            
            var trailPoints = PostProcess();

            this.VertexData = new VertexPositionColorTexture[trailPoints.Count * 2];

            float imgScale = ScrollingTrail.TRAIL_WIDTH;

            float pastDistance = this.TrailLength;

            var offsetDirection = new Vector3(0, 0, -1);

            var currPoint = trailPoints[0];
            Vector3 offset = Vector3.Zero;

            for (int i = 0; i < trailPoints.Count - 1; i++) {
                var nextPoint = trailPoints[i + 1];

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

            this.VertexData[trailPoints.Count * 2 - 1] = new VertexPositionColorTexture(fleftPoint,  Color.White, new Vector2(0f, pastDistance / (imgScale * 2) - 1));
            this.VertexData[trailPoints.Count * 2 - 2] = new VertexPositionColorTexture(frightPoint, Color.White, new Vector2(1f, pastDistance / (imgScale * 2) - 1));

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
                                              _playerFadeRadius,
                                              _fadeCenter,
                                              _fadeTexture,
                                              _tintColor);

            graphicsDevice.SetVertexBuffer(_vertexBuffer, 0);

            foreach (EffectPass trailPass in _sharedTrailEffect.CurrentTechnique.Passes) {
                trailPass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vertexBuffer.VertexCount - 2);
            }
        }

    }
}
