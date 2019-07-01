using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Annotations;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities {
    public class Trail : Entity {

        protected float         _trailLength = -1;
        protected List<Vector3> _trailPoints;
        protected Texture2D     _trailTexture;

        private VertexPositionColor[] VertexData { get; set; }

        private BasicEffect _renderEffect;
        
        public float DistanceFromCamera => float.MaxValue;

        public virtual IReadOnlyList<Vector3> TrailPoints => _trailPoints.AsReadOnly();

        public virtual float TrailLength {
            get {
                // Lazy load the trail length
                if (_trailLength < 0) {
                    _trailLength = 0;

                    for (int i = 0; i < _trailPoints.Count - 1; i++) {
                        _trailLength += Vector3.Distance(_trailPoints[i], _trailPoints[i + 1]);
                    }
                }

                return _trailLength;
            }
        }

        // TODO: Trail should not own this - only trails that support textures should define this
        public virtual Texture2D TrailTexture {
            get => _trailTexture;
            set => SetProperty(ref _trailTexture, value);
        }

        public Trail() : this(null) { /* NOOP */ }

        public Trail([CanBeNull] List<Vector3> trailPoints) {
            _trailPoints = trailPoints ?? new List<Vector3>();
            InitTrailPoints();
        }

        protected virtual void InitTrailPoints() {
            _renderEffect                    = _renderEffect ?? (BasicEffect)StandardEffect.Clone();
            _renderEffect.VertexColorEnabled = true;
            _renderEffect.TextureEnabled     = false;

            if (!_trailPoints.Any()) return;

            this.VertexData = new VertexPositionColor[_trailPoints.Count - 1];

            for (int i = 0; i < _trailPoints.Count - 1; i++) {
                this.VertexData[i] = new VertexPositionColor(_trailPoints[i], Color.Blue);
            }
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice) {
            /* NOOP */
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            foreach (var basicPass in _renderEffect.CurrentTechnique.Passes) {
                basicPass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip,
                                                  this.VertexData,
                                                  0,
                                                  this.VertexData.Length - 1);
            }
        }

    }
}
