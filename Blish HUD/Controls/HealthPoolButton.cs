using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Blish_HUD.Controls {

    // TODO: Inherit from LabelBase
    public class HealthPoolButton : Control {

        private const int BOTTOMEDGE_GAP = 17;
        private Texture2D HealthPoolSprite;
        private Texture2D HealthPoolPressedSprite;
        private bool IsBeingPressed;

        private string _text;
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        public HealthPoolButton() {
            HealthPoolSprite = HealthPoolSprite ?? Content.GetTexture("background_healthpool");
            HealthPoolPressedSprite = HealthPoolPressedSprite ?? Content.GetTexture("background_healthpool_pressed");
            this.Size = new Point(111, 111); // set static bounds.
            UpdateLocation(null, null);
            Graphics.SpriteScreen.Resized += UpdateLocation;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            this.IsBeingPressed = true;

            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            this.IsBeingPressed = false;

            base.OnLeftMouseButtonReleased(e);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        private void UpdateLocation(object sender, EventArgs e) {
            this.Location = new Point((Graphics.SpriteScreen.Width / 2 - this.Width / 2), (Graphics.SpriteScreen.Height - this.Height) - BOTTOMEDGE_GAP);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, HealthPoolSprite, new Rectangle(0, 0, this.Width, this.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);
            spriteBatch.DrawStringOnCtrl(this, this.Text, Content.DefaultFont14, new Rectangle(0, 0, this.Width, this.Height), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);
            if (IsBeingPressed) {
                spriteBatch.DrawOnCtrl(this, HealthPoolPressedSprite, new Rectangle(0, 0, this.Width, this.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);
            }
        }

    }
}
