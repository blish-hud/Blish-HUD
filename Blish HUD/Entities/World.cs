using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {

    public class World : Entity {
        
        public SynchronizedCollection<Entity> Entities { get; private set; }

        private IOrderedEnumerable<Entity> _sortedEntities;

        public World() : base() {
            this.Entities = new SynchronizedCollection<Entity>();
        }

        public override void Update(GameTime gameTime) {
            GameService.Debug.StartTimeFunc("Sorting 3D Entities");
            _sortedEntities = this.Entities.ToList().OrderByDescending(e => e.DistanceFromCamera);
            GameService.Debug.StopTimeFunc("Sorting 3D Entities");

            foreach (var entity in _sortedEntities) {
                entity.Update(gameTime);
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            foreach (var entity in _sortedEntities.Where(entity => entity.Visible)) {
                entity.Draw(graphicsDevice);
            }
        }
    }

}
