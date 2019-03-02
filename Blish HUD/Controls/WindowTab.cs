using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class WindowTab:Control {

        private bool _active = false;
        public bool Active { get { return _active; } set { _active = value; Invalidate(); } }

        public WindowTab() {
            this.Size = new Point(104, 52);
            
            this.LeftMouseButtonReleased += WindowTab_LeftMouseButtonReleased;
        }

        private void WindowTab_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            if (this.Active) return;

            this.Active = true;

            int varSe = new Random().Next(1, 5);

            Content.PlaySoundEffectByName($"audio\\tab-swap-{varSe}");
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.Active) {
                if (this.Parent != null) {
                    spriteBatch.Draw(Content.GetTexture("hero-background2"), bounds.OffsetBy(24, 0), this.Bounds.Add(52, 0, 110, 0), Color.White);
                }

                spriteBatch.Draw(Content.GetTexture("window-tab-active"), bounds, Color.White);
            } else {
                spriteBatch.Draw(Content.GetTexture("black-46x52"), Content.GetTexture("black-46x52").Bounds.OffsetBy(58, 0), Color.White);
            }

            spriteBatch.Draw(Content.GetTexture("156746"), new Rectangle(this.Width - 14 - 26, this.Height / 2 - 18, 32, 32), Color.White * (this.MouseOver || this.Active ? 1f : 0.8f));
        }

    }
}
