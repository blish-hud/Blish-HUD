using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {
    public class Marker : Primitives.Billboard {

        public int MapId { get; set; } = -1;

        public Marker(Texture2D image, Vector3 position, Vector2 size) : base(image, position, size) {
            //GameService.Input.MouseMoved += Input_MouseMoved;
        }

        private bool mouseOver = false;

        private void Input_MouseMoved(object sender, MouseEventArgs e) {
            var screenPosition = GameService.Graphics.GraphicsDevice.Viewport.Project(this.Position, GameService.Camera.Projection, GameService.Camera.View, Matrix.Identity);


            var xdist = screenPosition.X - e.MouseState.Position.X;
            var ydist = screenPosition.Y - e.MouseState.Position.Y;
            
            // Z < 1 means that the point is in front of the camera, not behind it
            mouseOver = screenPosition.Z < 1 && xdist < 2 && ydist < 2;


        }

        public override void Draw(GraphicsDevice graphicsDevice) {
            base.Draw(graphicsDevice);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            //this.Visible = !mouseOver || gameTime.TotalGameTime.TotalSeconds % 3 == 1;
        }

    }
}
