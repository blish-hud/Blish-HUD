using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
   public class Tooltip:Container {

        internal const int PADDING = 2;
        internal const int MOUSE_VERTICAL_MARGIN = 18;

        private const int BORDER_THICKNESS = 3;


        #region Load Static

        private static List<Tooltip> _allTooltips;

        private static Texture2D _textureTooltip;

        static Tooltip() {
            _textureTooltip = Content.GetTexture("tooltip");

            _allTooltips = new List<Tooltip>();

            Control.ActiveControlChanged += ControlOnActiveControlChanged;

            Input.MouseMoved += delegate(object sender, MouseEventArgs args) {
                if (Control.ActiveControl?.Tooltip != null) {
                    UpdateTooltipPosition(Control.ActiveControl.Tooltip);
                    Control.ActiveControl.Tooltip.Visible = true;
                }
            };
        }

        private static void ControlOnActiveControlChanged(object sender, ControlChangedEventArgs e) {
            foreach (var tooltip in _allTooltips) {
                tooltip.Hide();
            }

            if (e.ActivatedControl?.Tooltip != null) {
                UpdateTooltipPosition(e.ActivatedControl.Tooltip);
                e.ActivatedControl.Tooltip.Visible = true;
            }
        }

        private static void UpdateTooltipPosition(Tooltip tooltip) {
            int topPos = Input.MouseState.Position.Y - Tooltip.MOUSE_VERTICAL_MARGIN - tooltip.Height > 0
                             ? -Tooltip.MOUSE_VERTICAL_MARGIN - tooltip.Height
                             : Tooltip.MOUSE_VERTICAL_MARGIN * 2;

            int leftPos = Input.MouseState.Position.X + tooltip.Width < Graphics.SpriteScreen.Width
                              ? 0
                              : -tooltip.Width;

            tooltip.Location = Input.MouseState.Position + new Point(leftPos, topPos);
        }

        #endregion


        public Control CurrentControl { get; set; }

        public Tooltip() : base() {
            this.Visible = false;
            this.Parent = GameService.Graphics.SpriteScreen;
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;

            this.ChildAdded   += Tooltip_ChildChanged;
            this.ChildRemoved += Tooltip_ChildChanged;

            //Input.MouseMoved += delegate {
            //    if (this.Visible && !this.CurrentControl.MouseOver) {
            //        this.Visible = false;
            //    }
            //};

            _allTooltips.Add(this);
        }

        private void Tooltip_ChildChanged(object sender, ChildChangedEventArgs e) {
            Invalidate();

            // Ensure we don't miss it if a child control is resized or is moved
            if (e.Added) {
                e.ChangedChild.Resized += delegate { Invalidate(); };
                e.ChangedChild.Moved += delegate { Invalidate(); };
            } else {
                // TODO: Remove handlers
            }
        }

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            if (this.CurrentControl != null && !this.CurrentControl.Visible) {
                this.Visible = false;
                this.CurrentControl = null;
            }
        }

        public override void RecalculateLayout() {
            var visibleChildren = _children.Where(c => c.Visible).ToList();

            int boundsWidth  = 0;
            int boundsHeight = 0;

            if (visibleChildren.Count > 0) {
                boundsWidth  = visibleChildren.Max(c => c.Right);
                boundsHeight = visibleChildren.Max(c => c.Bottom);
            }

            this.Size = new Point(BORDER_THICKNESS + PADDING + boundsWidth  + PADDING + BORDER_THICKNESS,
                                  BORDER_THICKNESS + PADDING + boundsHeight + PADDING + BORDER_THICKNESS);

            this.ContentRegion = new Rectangle(BORDER_THICKNESS + PADDING,
                                               BORDER_THICKNESS + PADDING,
                                               _size.X - PADDING - BORDER_THICKNESS,
                                               _size.Y - PADDING - BORDER_THICKNESS);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureTooltip, bounds.Add(0, 0, -3, -3), new Rectangle(0, 0, this.Width - 3, this.Height - 3));
            spriteBatch.DrawOnCtrl(this, _textureTooltip, new Rectangle(bounds.Right - 3, bounds.Top, 3, bounds.Height), new Rectangle(0, 3, 3, this.Height - 3));
            spriteBatch.DrawOnCtrl(this, _textureTooltip, new Rectangle(bounds.Left, bounds.Bottom - 3, bounds.Width, 3), new Rectangle(3, 0, this.Width - 6, 3));
        }

    }
}
