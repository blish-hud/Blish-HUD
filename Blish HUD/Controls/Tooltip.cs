﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
   public class Tooltip : Container {

        private const int PADDING = 2;

        private const int MOUSE_VERTICAL_MARGIN = 18;

        #region Load Static

        private static readonly Thickness _contentEdgeBuffer;

        private static readonly List<Tooltip> _allTooltips;

        private static readonly Texture2D _textureTooltip;

        static Tooltip() {
            _contentEdgeBuffer = new Thickness(4, 4, 3, 6);

            _textureTooltip = Content.GetTexture("tooltip");

            _allTooltips = new List<Tooltip>();

            Control.ActiveControlChanged += ControlOnActiveControlChanged;

            Input.MouseMoved += delegate(object sender, MouseEventArgs args) {
                if (ActiveControl?.Tooltip != null) {
                    ActiveControl.Tooltip.CurrentControl = ActiveControl;
                    UpdateTooltipPosition(ActiveControl.Tooltip);

                    if (!ActiveControl.Tooltip.Visible)
                        ActiveControl.Tooltip.Show();
                }
            };
        }

        private static Control _prevControl;

        private static void ControlOnActiveControlChanged(object sender, ControlActivatedEventArgs e) {
            foreach (var tooltip in _allTooltips) {
                tooltip.Hide();
            }

            if (_prevControl != null) {
                _prevControl.Hidden   -= ActivatedControlOnHidden;
                _prevControl.Disposed -= ActivatedControlOnHidden;
            }

            _prevControl = e.ActivatedControl;

            if (_prevControl != null) {
                e.ActivatedControl.Hidden   += ActivatedControlOnHidden;
                e.ActivatedControl.Disposed += ActivatedControlOnHidden;
            }
        }

        private static void ActivatedControlOnHidden(object sender, EventArgs e) {
            foreach (var tooltip in _allTooltips) {
                tooltip.Hide();
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

        private Glide.Tween _animFadeLifecycle;

        public Tooltip() : base() {
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;

            this.Padding = new Thickness(PADDING);

            this.ChildAdded   += Tooltip_ChildChanged;
            this.ChildRemoved += Tooltip_ChildChanged;

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

        public override void UpdateContainer(GameTime gameTime) {
            if (this.CurrentControl != null && !this.CurrentControl.Visible) {
                this.Visible = false;
                this.CurrentControl = null;
            }
        }

        /// <summary>
        /// Shows the tooltip at the provided <see cref="x"/> and <see cref="y"/> coordinates.
        /// </summary>
        public void Show(int x, int y) {
            this.Show(new Point(x, y));
        }

        /// <summary>
        /// Shows the tooltip at the provided <see cref="location"/>.
        /// </summary>
        public void Show(Point location) {
            this.Location = location;

            this.Show();
        }

        /// <inheritdoc />
        public override void Show() {
            this.Opacity = 0f;

            if (_animFadeLifecycle == null) {
                _animFadeLifecycle = Animation.Tweener.Tween(this, new {Opacity = 1f}, 0.1f);
            }

            this.Parent = Graphics.SpriteScreen;

            base.Show();
        }

        /// <inheritdoc />
        public override void Hide() {
            _animFadeLifecycle?.Cancel();
            _animFadeLifecycle = null;

            this.Parent = null;

            base.Hide();
        }

        public override void RecalculateLayout() {
            var visibleChildren = _children.Where(c => c.Visible).ToList();

            int boundsWidth  = 0;
            int boundsHeight = 0;

            if (visibleChildren.Count > 0) {
                boundsWidth  = visibleChildren.Max(c => c.Right);
                boundsHeight = visibleChildren.Max(c => c.Bottom);
            }

            this.Size = new Point((int)(_contentEdgeBuffer.Left + boundsWidth + _contentEdgeBuffer.Right),
                                  (int)(_contentEdgeBuffer.Top + boundsHeight + _contentEdgeBuffer.Bottom));

            this.ContentRegion = new Rectangle((int)_contentEdgeBuffer.Left,
                                               (int)_contentEdgeBuffer.Top,
                                               (int)(_size.X - _contentEdgeBuffer.Left - _contentEdgeBuffer.Right),
                                               (int)(_size.Y - _contentEdgeBuffer.Top - _contentEdgeBuffer.Bottom));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureTooltip, bounds, new Rectangle(3, 4, _size.X, _size.Y), Color.White * 0.98f);

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, _size.X - 2, 3).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, _size.X - 2, 1).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.Black * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 3, 1, 3, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 2, 1, 1, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 3, _size.X - 2, 3).Add(-PADDING, PADDING, PADDING * 2, 0), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 2, _size.X - 2, 1).Add(-PADDING, PADDING, PADDING * 2, 0), Color.Black * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 3, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, 1, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.6f);
        }

    }
}
