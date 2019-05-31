using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public class Mesh:Entity {

        private Model _model;

        public Vector3 Size { get; set; } = Vector3.One;

        public Mesh(Model model) : base() {
            _model = model;
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            foreach (var mesh in _model.Meshes) {
                foreach (BasicEffect effect in mesh.Effects) {
                    //effect.EnableDefaultLighting();
                    //effect.PreferPerPixelLighting = true;
                    //effect.TextureEnabled = true;
                    //effect.Texture = MainLoop.traffic_texture;
                    //effect.SpecularPower = 1f;
                    //effect.LightingEnabled = true;
                    effect.VertexColorEnabled = true;

                    //effect.AmbientLightColor = Vector3.One;

                    //effect.EmissiveColor = new Vector3(1);

                    //effect.DirectionalLight0.DiffuseColor = Vector3.One;
                    //effect.DirectionalLight0.SpecularColor = Vector3.One;
                    //effect.DirectionalLight0.Direction = Vector3.Normalize(Services.Services.Camera.Position - Position);



                    effect.View = GameService.Camera.View;
                    effect.Projection = GameService.Camera.Projection;
                    effect.World = Matrix.CreateScale(this.Size) * Matrix.CreateTranslation(this.Position);
                }

                mesh.Draw();
            }
        }

        public override void Update(GameTime gameTime) {
            //Position = Services.Services.Player.Position + new Vector3(0, 0, 2);
        }

    }
}
