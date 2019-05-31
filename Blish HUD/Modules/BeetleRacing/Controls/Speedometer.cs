using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.BeetleRacing.Controls {

    // TODO: Apply alpha mask to speedometer
    public class Speedometer:Control {

        public int MinSpeed = 0;
        public float MaxSpeed = 2200;
        public float Speed { get; set; } = 0;

        public bool ShowSpeedValue { get; set; } = false;

        public Speedometer() {
            this.Size = new Point(128, 128);

            UpdateLocation(null, null);
            Graphics.SpriteScreen.Resized += UpdateLocation;
        }

        private void UpdateLocation(object sender, EventArgs e) {
            this.Location = new Point(Graphics.SpriteScreen.Width / 2 - 64, Graphics.SpriteScreen.Height - 218);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            Invalidate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            float ang = (float)((4 + this.Speed / MaxSpeed * 2));

            spriteBatch.DrawOnCtrl(this,
                                   Content.GetTexture("speed-fill"),
                                   new Rectangle(_size.X / 2, _size.Y + 15, 150, 203),
                                   null,
                                   Color.Lerp(Color.GreenYellow, Color.Red, this.Speed / MaxSpeed),
                                   ang,
                                   new Vector2(Content.GetTexture("speed-fill").Bounds.Width / 2, 141));

            spriteBatch.DrawOnCtrl(this,
                                   Content.GetTexture("1060345-2"),
                                   _size.InBounds(bounds),
                                   null,
                                   Color.White,
                                   0f,
                                   Vector2.Zero);

            if (this.ShowSpeedValue) {
                spriteBatch.DrawStringOnCtrl(this,
                                         Math.Round(this.Speed).ToString(),
                                         Content.DefaultFont14,
                                         new Rectangle(0, 0, _size.X, 50),
                                         Color.White,
                                         false,
                                         DrawUtil.HorizontalAlignment.Center,
                                         DrawUtil.VerticalAlignment.Bottom);
            }
        }

    }
}
