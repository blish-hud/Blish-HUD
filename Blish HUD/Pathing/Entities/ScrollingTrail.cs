using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Annotations;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D11;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace Blish_HUD.Pathing.Entities {
    public class ScrollingTrail : Trail, ITrail {


        public const float TRAIL_WIDTH = 40 * 0.0254f;

        private float _animationSpeed = 1;

        private float _fadeNear = 700;
        private float _fadeFar = 900;
        private float _scale = 1;
        private float _opacity = 1;

        public float AnimationSpeed {
            get => _animationSpeed;
            set {
                if (SetProperty(ref _animationSpeed, value))
                    _sections.ForEach(s => s.AnimationSpeed = value);
            }
        }

        public override Texture2D TrailTexture {
            get => _trailTexture;
            set {
                if (SetProperty(ref _trailTexture, value))
                    _sections.ForEach(s => s.TrailTexture = value);
            }
        }

        public float FadeNear {
            get => _fadeNear;
            set {
                if (SetProperty(ref _fadeNear, value))
                    _sections.ForEach(s => s.FadeNear = value);
            }
        }

        public float FadeFar {
            get => _fadeFar;
            set {
                if (SetProperty(ref _fadeFar, value))
                    _sections.ForEach(s => s.FadeFar = value);
            }
        }

        public float Scale {
            get => _scale;
            set {
                if (SetProperty(ref _scale, value))
                    _sections.ForEach(s => s.Scale = value);
            }
        }

        public override float Opacity {
            get => _opacity;
            set {
                if (SetProperty(ref _opacity, value))
                    _sections.ForEach(s => s.Opacity = value);
            }
        }

        private readonly List<ScrollingTrailSection> _sections;

        public ScrollingTrail(List<List<Vector3>> trailSections) {
            _sections = new List<ScrollingTrailSection>();

            AddSections(trailSections);
        }

        public ScrollingTrail() {
            _sections = new List<ScrollingTrailSection>();
        }

        public void AddSections(List<ScrollingTrailSection> newSections) {
            newSections.ForEach(AddSection);
        }

        public void AddSections(List<List<Vector3>> newSectionsPoints) {
            newSectionsPoints.ForEach(AddSection);
        }

        public void AddSection(ScrollingTrailSection newSection) {
            newSection.AnimationSpeed = _animationSpeed;
            newSection.FadeFar = _fadeFar;
            newSection.FadeNear = _fadeNear;
            newSection.Scale = _scale;
            newSection.TrailTexture = _trailTexture;
            newSection.Opacity = _opacity;

            _sections.Add(newSection);
        }

        public void AddSection(List<Vector3> newSectionPoints) {
            AddSection(new ScrollingTrailSection(newSectionPoints));
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            _sections.ForEach(s => s.Update(gameTime));
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            _sections.ForEach(s => s.Draw(graphicsDevice));
        }


    }
}
