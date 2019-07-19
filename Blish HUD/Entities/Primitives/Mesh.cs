using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities.Primitives {
    public class Mesh : Entity {

        private Model _model;

        public Vector3 Size { get; set; } = Vector3.One;

        public Mesh(Model model) : base() {
            _model = model;
        }

        /// <inheritdoc />
        public override void HandleRebuild(GraphicsDevice graphicsDevice) {
            // NOOP
        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            foreach (var mesh in _model.Meshes) {
                foreach (BasicEffect effect in mesh.Effects) {
                    effect.View       = GameService.Camera.View;
                    effect.Projection = GameService.Camera.Projection;
                    effect.World      = Matrix.CreateScale(this.Size) * Matrix.CreateTranslation(this.Position);
                    effect.Alpha      = _opacity;
                }

                mesh.Draw();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing) {
            if (!_disposed && disposing) {
                _model = null;
            }

            base.Dispose(disposing);
        }

    }
}
