using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class LoadingSpinner:Control {

        private const string DRAWATLAS = "spinner-atlas";
        private const int DRAWLENGTH = 64;

        private Rectangle _activeAtlasRegion = new Rectangle(0, 416, 256, 32);
        private Rectangle ActiveAtlasRegion {
            get => _activeAtlasRegion;
            set {
                _activeAtlasRegion = value;
                Invalidate();
            }
        }

        public int Rotation { get; set; } = 0;

        private EaseAnimation SpinAnimation;
        private Glide.Tween SpinAnimation2;

        public LoadingSpinner() {
            this.Size = new Point(DRAWLENGTH, DRAWLENGTH);

            SpinAnimation2 = Animation.Tweener.Tween(this, new { Rotation = 64 }, 3).Repeat().Round();
        }

        public override void Update(GameTime gameTime) {
            this.ActiveAtlasRegion = new Rectangle(DRAWLENGTH * this.Rotation, 0, DRAWLENGTH, DRAWLENGTH); //SpinAnimation.CurrentValueInt, 0, DRAWLENGTH, DRAWLENGTH);

            base.Update(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // TODO: Add this texture in with the rest of the UI elements in the ControlUI atlas
            spriteBatch.Draw(Content.GetTexture(DRAWATLAS), bounds, this.ActiveAtlasRegion, Color.White);
        }

    }
}
