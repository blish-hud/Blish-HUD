using Blish_HUD.Controls;
using Blish_HUD.Input;
using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.GameServices.Gw2WebApi.Gw2Auth.Controls {
    internal class Gw2AuthButton : Image {
        public Gw2AuthButton() {
            this.Texture = GameService.Content.GetTexture("vendor/gw2auth/gw2auth_64x64");
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            base.OnLeftMouseButtonReleased(e);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            this.Top   -= 2;
            this.Left   -= 2;
            this.Width  += 4;
            this.Height += 4;
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            this.Top                              += 2;
            this.Left                             += 2;
            this.Width                            -= 4;
            this.Height                           -= 4;
            base.OnMouseLeft(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            base.Paint(spriteBatch, bounds);
        }

        protected override void DisposeControl() {
            base.DisposeControl();
        }
    }
}
