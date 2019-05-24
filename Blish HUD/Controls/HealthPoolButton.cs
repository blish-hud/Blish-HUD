using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Blish_HUD.Controls {

    public class HealthPoolButton : Container {

        private const int BOTTOMEDGE_GAP = 17;
        private Texture2D HealthPoolSprite;
        private Texture2D HealthPoolPressedSprite;
        private bool IsBeingPressed;

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (string.Equals(_text, value)) return;
                _text = value;
                OnPropertyChanged();
            }
        }
        public HealthPoolButton() {
            HealthPoolSprite = HealthPoolSprite ?? Content.GetTexture("background_healthpool");
            HealthPoolPressedSprite = HealthPoolPressedSprite ?? Content.GetTexture("background_healthpool_pressed");
            this.Size = new Point(111, 111); // set static bounds.
            UpdateLocation(null, null);
            Graphics.SpriteScreen.Resized += UpdateLocation;
        }
        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            this.IsBeingPressed = true;
            base.OnLeftMouseButtonPressed(e);
        }
        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            this.IsBeingPressed = false;
            base.OnLeftMouseButtonReleased(e);
        }
        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }
        private void UpdateLocation(object sender, EventArgs e) {
            this.Location = new Point((Graphics.SpriteScreen.Width / 2 - this.Width / 2), (Graphics.SpriteScreen.Height - this.Height) - BOTTOMEDGE_GAP);
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, HealthPoolSprite, new Rectangle(0, 0, this.Width, this.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);
            spriteBatch.DrawStringOnCtrl(this, this.Text, Content.DefaultFont14, new Rectangle(0, 0, this.Width, this.Height), Color.White, false, true, 1, Utils.DrawUtil.HorizontalAlignment.Center, Utils.DrawUtil.VerticalAlignment.Middle);
            if (IsBeingPressed)
            {
                spriteBatch.DrawOnCtrl(this, HealthPoolPressedSprite, new Rectangle(0, 0, this.Width, this.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);
            }
        }
    }
}
