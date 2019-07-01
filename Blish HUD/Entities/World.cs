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

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice) {
            /* NOOP - world does not need to rebuild */ 
        }

        public override void DoUpdate(GameTime gameTime) {
            UpdateEntitySort();

            foreach (var entity in _sortedEntities) {
                entity.DoUpdate(gameTime);
            }
        }

        public override void DoDraw(GraphicsDevice graphicsDevice) {
            graphicsDevice.BlendState        = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.SamplerStates[0]  = SamplerState.LinearWrap;
            graphicsDevice.RasterizerState   = RasterizerState.CullCounterClockwise;

            foreach (var entity in _sortedEntities.Where(entity => entity.Visible)) {
                entity.DoDraw(graphicsDevice);
            }
        }
    }

}
