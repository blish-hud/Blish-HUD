using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {

    public class World:Entity {
        
        public Collection<Entity> Entities { get; private set; }

        public World() : base() {
            this.Entities = new Collection<Entity>();
        }

        public override void Update(GameTime gameTime) {
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
