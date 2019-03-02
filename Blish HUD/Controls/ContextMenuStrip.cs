using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class ContextMenuStrip:Container {

        private const int BORDER_PADDING = 2;

        private const int ITEM_WIDTH          = 160;
        private const int ITEM_HEIGHT         = 22;
        private const int ITEM_VERTICALMARGIN = 6;

        private const int CONTROL_WIDTH = BORDER_PADDING + ITEM_WIDTH + BORDER_PADDING;

        private static Texture2D _edgeSprite;

        public ContextMenuStrip() {
            _edgeSprite = _edgeSprite ?? Content.GetTexture("scrollbar-track");
            
            this.Visible = false;
            this.Width = CONTROL_WIDTH;
            this.ZIndex = Screen.CONTEXTMENU_BASEINDEX;
            this.Parent = GameService.Graphics.SpriteScreen;

            Input.LeftMouseButtonPressed += MouseButtonPressed;
            Input.RightMouseButtonPressed += MouseButtonPressed;
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);
            OnChildMembershipChanged(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);
            OnChildMembershipChanged(e);
        }

        private void MouseButtonPressed(object sender, MouseEventArgs e) {
            if (!this.MouseOver)
                this.Visible = false;
        }

        public ContextMenuStripItem AddMenuItem(string text) {
            return new ContextMenuStripItem() {
                Text = text,
                Parent = this
            };
        }

        private void OnChildMembershipChanged(ChildChangedEventArgs e) {
            if (e.Added) {
                if (!(e.ChangedChild is ContextMenuStripItem newChild)) {
                    e.Cancel = true;
                    return;
                }

                newChild.Height = ITEM_HEIGHT;
                newChild.Width = this.Width - BORDER_PADDING * 2;
                newChild.Left = BORDER_PADDING;
            }

            int lastBottom = -4;
            e.ResultingChildren.ForEach(child => {
                                            child.Top = lastBottom + ITEM_VERTICALMARGIN;
                                            lastBottom = child.Bottom;
                                        });

            this.Height = lastBottom + BORDER_PADDING;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(ContentService.Textures.Pixel, 
                             new Rectangle(BORDER_PADDING,
                                           BORDER_PADDING,
                                           this.Width - BORDER_PADDING * 2,
                                           this.Height - BORDER_PADDING * 2),
                             Color.FromNonPremultiplied(33, 32, 33, 255));

            // Left line
            spriteBatch.Draw(_edgeSprite,
                             new Rectangle(0, 1, _edgeSprite.Width, this.Height - BORDER_PADDING),
                             new Rectangle(0, 1, _edgeSprite.Width, this.Height - BORDER_PADDING),
                             Color.White * 0.8f);

            // Top line
            spriteBatch.Draw(_edgeSprite,
                             new Rectangle(1, BORDER_PADDING, _edgeSprite.Width, this.Width - BORDER_PADDING),
                             new Rectangle(1, BORDER_PADDING, _edgeSprite.Width, this.Width - BORDER_PADDING),
                             Color.White * 0.8f,
                             -MathHelper.PiOver2,
                             Vector2.Zero,
                             SpriteEffects.None,
                             0f);

            // Bottom line
            spriteBatch.Draw(_edgeSprite,
                             new Rectangle(1, this.Height, _edgeSprite.Width, this.Width - BORDER_PADDING),
                             new Rectangle(1, _edgeSprite.Height / 2, _edgeSprite.Width, this.Width - BORDER_PADDING),
                             Color.White * 0.8f,
                             -MathHelper.PiOver2,
                             Vector2.Zero,
                             SpriteEffects.None,
                             0f);

            // Right line
            spriteBatch.Draw(_edgeSprite,
                             new Rectangle(this.Width - _edgeSprite.Width, 1, _edgeSprite.Width, this.Height - 2),
                             new Rectangle(0, 1, _edgeSprite.Width, this.Height - 2),
                             Color.White * 0.8f);
        }
    }

}
