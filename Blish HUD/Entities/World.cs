using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {

    public class World:Entity {
        
        public List<Entity> Entities { get; private set; }

        public World() : base() {
            this.Entities = new List<Entity>();
        }

        public override void Update(GameTime gameTime) {
            GameService.Debug.StartTimeFunc("Sorting 3D Entities");
            this.Entities = this.Entities.OrderByDescending(e => e.DistanceFromCamera).ToList();
            GameService.Debug.StopTimeFunc("Sorting 3D Entities");

            foreach (var entity in this.Entities) {
                entity.Update(gameTime);
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            foreach (var entity in this.Entities) {
                if (entity.Visible) {
                    entity.Draw(graphicsDevice);
                }
            }
        }
    }

}
