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
            this.Top   -= 3;
            this.Left   -= 3;
            this.Width  += 3;
            this.Height += 3;
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            this.Top                              += 3;
            this.Left                             += 3;
            this.Width                            -= 3;
            this.Height                           -= 3;
            this.SpriteBatchParameters.Effect     =  null;
            this.SpriteBatchParameters.BlendState =  null;
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
