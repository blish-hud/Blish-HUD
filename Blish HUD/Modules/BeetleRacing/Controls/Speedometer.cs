﻿using System;
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
        public float MaxSpeed = 50;
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

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            Invalidate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            float ang = (float)((4 + this.Speed / MaxSpeed * 2));

            spriteBatch.Draw(Content.GetTexture("speed-fill"), new Rectangle(bounds.Width / 2, bounds.Y + 140, 150, 203), null, Color.GreenYellow, ang, new Vector2(Content.GetTexture("speed-fill").Bounds.Width / 2, 141), SpriteEffects.None, 1); // new Vector2(64, 203), SpriteEffects.None, 0);
            spriteBatch.Draw(Content.GetTexture("1060345-2"), bounds, bounds, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);

            if (this.ShowSpeedValue) {
                Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14,
                    ((int) Math.Round(this.Speed * 8, 0)).ToString(), new Rectangle(0, 0, this.Width, 50),
                    Color.White, DrawUtil.HorizontalAlignment.Center, DrawUtil.VerticalAlignment.Bottom);
            }
        }

    }
}
