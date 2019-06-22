using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {

    public class World : Entity {
        
        public SynchronizedCollection<Entity> Entities { get; private set; }

        private IOrderedEnumerable<Entity> _sortedEntities;

        public World() : base() {
            this.Entities = new SynchronizedCollection<Entity>();
            UpdateEntitySort();
        }

        private void UpdateEntitySort() {
            _sortedEntities = this.Entities.ToList().OrderByDescending(e => e.DistanceFromCamera);
        }

        public override void Update(GameTime gameTime) {
            UpdateEntitySort();

            foreach (var entity in _sortedEntities) {
                entity.Update(gameTime);
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            graphicsDevice.BlendState        = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.SamplerStates[0]  = SamplerState.LinearWrap;
            graphicsDevice.RasterizerState   = RasterizerState.CullNone;

            foreach (var entity in _sortedEntities.Where(entity => entity.Visible)) {
                entity.Draw(graphicsDevice);
            }
        }
    }

}
