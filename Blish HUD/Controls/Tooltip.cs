using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
   public class Tooltip:Container {

        internal const int PADDING = 10;
        internal const int MOUSE_VERTICAL_MARGIN = 18;

        public Control CurrentControl { get; set; }

        private bool _checkSize = true;

        public Tooltip() : base() {
            this.Visible = false;
            this.Parent = GameService.Graphics.SpriteScreen;
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;

            this.ChildAdded += Tooltip_ChildChanged;
            this.ChildRemoved += Tooltip_ChildChanged;

            Input.MouseMoved += delegate {
                if (this.Visible) {
                    if (!this.CurrentControl.MouseOver) {
                        this.Visible = false;
                    }
                }
            };
        }

        private void Tooltip_ChildChanged(object sender, ChildChangedEventArgs e) {
            // In case multiple children are added or removed, we "queue" the size update so that it only happens once
            // when the next update occurs
            _checkSize = true;

            // TODO: Double check the logic for this auto-tooltip resize
            // Ensure we don't miss it if a child control is resized or is moved
            if (e.Added) {
                e.ChangedChild.Resized += delegate { _checkSize = true; UpdateSize(e.ChangedChild); };
                e.ChangedChild.Moved += delegate { _checkSize = true; UpdateSize(e.ChangedChild); };
            }
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (_checkSize) UpdateSize();

            if (this.CurrentControl != null && !this.CurrentControl.Visible) {
                this.Visible = false;
                this.CurrentControl = null;
            }
        }

        private void UpdateSize(params Control[] additionalChildren) {
            var childList = this.Children.ToList();
            childList.AddRange(additionalChildren);

            int boundsRight = PADDING;
            int boundsBottom = PADDING;

            if (childList.Any()) {
                boundsRight  = childList.Where(c => c.Visible).Max(c => c.Right);
                boundsBottom = childList.Where(c => c.Visible).Max(c => c.Bottom);
            }

            this.Size = new Point(boundsRight + PADDING, boundsBottom + PADDING);

            _checkSize = false;
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            var tooltipBack = Content.GetTexture("tooltip");

            spriteBatch.Draw(tooltipBack, bounds.Add(0, 0, -3, -3), new Rectangle(0, 0, this.Width - 3, this.Height - 3), Color.White);
            spriteBatch.Draw(tooltipBack, new Rectangle(bounds.Right - 3, bounds.Top, 3, bounds.Height), new Rectangle(0, 3, 3, this.Height - 3), Color.White);
            spriteBatch.Draw(tooltipBack, new Rectangle(bounds.Left, bounds.Bottom - 3, bounds.Width, 3), new Rectangle(3, 0, this.Width - 6, 3), Color.White);
        }

    }
}
