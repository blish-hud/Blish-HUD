using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Blish_HUD.Modules {
    public class Compass : Module {

        private Entities.Primitives.Billboard northBb;
        private Entities.Primitives.Billboard eastBb;
        private Entities.Primitives.Billboard southBb;
        private Entities.Primitives.Billboard westBb;

        private Color DirectionColor;

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "(General) Tactical Compass Module",
                "bh.general.compass",
                "Displays a basic indicator at your feet that shows you North, East, South, and West.",
                "LandersXanders.1235",
                "1"
            );
        }

        public override void DefineSettings(Settings settings) {

        }

        private float compassSize = 0.5f;

        protected override void OnEnabled() {
            base.OnEnabled();

            northBb = new Entities.Primitives.Billboard(GameService.Content.GetTexture("north"), GameService.Player.Position + new Vector3(0, 1, 0), new Vector2(compassSize, compassSize));
            eastBb = new Entities.Primitives.Billboard(GameService.Content.GetTexture("east"), GameService.Player.Position + new Vector3(1, 0, 0), new Vector2(compassSize, compassSize));
            southBb = new Entities.Primitives.Billboard(GameService.Content.GetTexture("south"), GameService.Player.Position + new Vector3(0, -1, 0), new Vector2(compassSize, compassSize));
            westBb = new Entities.Primitives.Billboard(GameService.Content.GetTexture("west"), GameService.Player.Position + new Vector3(-1, 0, 0), new Vector2(compassSize, compassSize));

            GameService.Graphics.World.Entities.Add(northBb);
            GameService.Graphics.World.Entities.Add(eastBb);
            GameService.Graphics.World.Entities.Add(southBb);
            GameService.Graphics.World.Entities.Add(westBb);
        }

        protected override void OnDisabled() {
            base.OnDisabled();

            GameService.Graphics.World.Entities.Remove(northBb);
            GameService.Graphics.World.Entities.Remove(eastBb);
            GameService.Graphics.World.Entities.Remove(southBb);
            GameService.Graphics.World.Entities.Remove(westBb);
        }

        public override void Update(GameTime gameTime) {
            northBb.Position = GameServices.GetService<PlayerService>().Position + new Vector3(0, 1, 0);
            eastBb.Position = GameServices.GetService<PlayerService>().Position + new Vector3(1, 0, 0);
            southBb.Position = GameServices.GetService<PlayerService>().Position + new Vector3(0, -1, 0);
            westBb.Position = GameServices.GetService<PlayerService>().Position + new Vector3(-1, 0, 0);

            northBb.Opacity = Math.Min(1 - GameService.Camera.Forward.Y, 1f);
            eastBb.Opacity = Math.Min(1 - GameService.Camera.Forward.X, 1f);
            southBb.Opacity = Math.Min(1 + GameService.Camera.Forward.Y, 1f);
            westBb.Opacity = Math.Min(1 + GameService.Camera.Forward.X, 1f);
        }

    }
}
