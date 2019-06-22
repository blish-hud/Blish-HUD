using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public class Mesh:Entity {

        private readonly Model _model;

        private Texture2D _bananaTexture;

        public Vector3 Size { get; set; } = Vector3.One / 5;

        public Mesh(Model model) : base() {
            _model = model;
            //_bananaTexture = GameService.Content.ContentManager.Load<Texture2D>(@"models\banana");
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            foreach (var mesh in _model.Meshes) {
                foreach (BasicEffect effect in mesh.Effects) {
                    //effect.EnableDefaultLighting();
                    //effect.PreferPerPixelLighting = true;
                    ////effect.TextureEnabled = true;
                    //effect.TextureEnabled = true;
                    //effect.Texture = _bananaTexture;
                    //effect.SpecularPower = 1f;
                    //effect.LightingEnabled = true;
                    //effect.VertexColorEnabled = true;

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
            //Position = GameService.Player.Position + new Vector3(0, 0, 2);
        }

    }
}
