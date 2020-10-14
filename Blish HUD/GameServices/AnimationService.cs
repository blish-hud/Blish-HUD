using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class AnimationService:GameService {

        public Glide.Tweener Tweener { get; private set; }

        protected override void Initialize() {
            Glide.Tween.TweenerImpl.SetLerper<Library.Glide.CustomLerpers.PointLerper>(typeof(Point));

            this.Tweener = new Glide.Tweener();
        }

        protected override void Load() { /* NOOP */ }

        protected override void Unload() { /* NOOP */ }

        protected override void Update(GameTime gameTime) {
            this.Tweener.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
