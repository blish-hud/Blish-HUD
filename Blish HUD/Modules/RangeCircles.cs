using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules {
    public class RangeCircles : Module {

        private Dictionary<int, Texture2D> CircleCache = new Dictionary<int, Texture2D>();
        private List<Entities.Primitives.Face> RangeCirclesObjs = new List<Entities.Primitives.Face>();
        
        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "(General) Range Circles Module",
                "bh.general.rangecircles",
                "Displays range circles to help give a visual indicator of what the range is for your attacks.",
                "LandersXanders.1235",
                "1"
            );
        }

        public override void DefineSettings(Settings settings) {

        }

        protected override void OnEnabled() {
            foreach (int rad in new int[] { 90, 120, 180, 240, 300, 400, 600, 900, 1000, 1200, 1500 }) {
                if (!CircleCache.ContainsKey(rad))
                    CircleCache.Add(rad, Utils.DrawUtil.DrawCircle(GameService.Graphics.GraphicsDevice, rad, 4));
            }

            foreach (int enabledCirc in new int[] { 240, 900, 1000, 1500 }) {
                RangeCirclesObjs.Add(new Entities.Primitives.Face() {
                    Size    = new Vector2(enabledCirc * 2f / 39f),
                    Texture = CircleCache[enabledCirc],
                });
            }

            // TODO: This class needs some work before we continue to render any of this
            foreach (var rangeCirclesObj in RangeCirclesObjs) {
                GameService.Graphics.World.Entities.Add(rangeCirclesObj);
            }
        }

        protected override void OnDisabled() {
            foreach (var circ in RangeCirclesObjs) {
                GameService.Graphics.World.Entities.Remove(circ);
            }

            RangeCirclesObjs.Clear();
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            foreach (var circ in RangeCirclesObjs) {
                circ.Position = GameService.Player.Position - new Vector3(0, 0, 0);
            }
        }

    }
}
