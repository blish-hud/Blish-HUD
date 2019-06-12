using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used to group collections of controls. Can have an accented border and title, if enabled.
    /// </summary>
    public class Panel : Container {

        // Used when border is enabled
        public const int TOP_MARGIN    = 0;
        public const int RIGHT_MARGIN  = 5;
        public const int BOTTOM_MARGIN = 10;
        public const int LEFT_MARGIN   = 8;

        protected bool _canScroll = false;
        public bool CanScroll {
            get => _canScroll;
            set {
                if (!SetProperty(ref _canScroll, value)) return;

                UpdateRegions();
                UpdateScrollbar();
            }
        }
        
        protected string _title;
        public string Title {
            get => _title;
            set {
                if (SetProperty(ref _title, value))
                    UpdateRegions();
            }
        }

        protected bool _showBorder;
        public bool ShowBorder {
            get => _showBorder;
            set {
                if (SetProperty(ref _showBorder, value))
                    UpdateRegions();
            }
        }

        private Scrollbar _panelScrollbar;

        public delegate void BuildUIDelegate(Panel buildPanel, object obj);

        public void NavigateToBuiltPanel(BuildUIDelegate buildCall, object obj) {
            this.Children.ToList().ForEach(c => c.Dispose());

            var buildPanel = new Panel() {
                Size = _size
            };

            buildCall(buildPanel, obj);

            buildPanel.Parent = this;
        }

        protected override void OnMoved(MovedEventArgs e) {
            base.OnMoved(e);

            // Mostly needed to update the scrollbar location, if it's visible
            UpdateRegions();
        }

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);
            UpdateRegions();
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) {
            base.OnChildAdded(e);

            e.ChangedChild.Resized += UpdateContentRegionBounds;
            e.ChangedChild.Moved   += UpdateContentRegionBounds;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e) {
            base.OnChildRemoved(e);

            e.ChangedChild.Resized -= UpdateContentRegionBounds;
            e.ChangedChild.Moved   -= UpdateContentRegionBounds;
        }

        private void UpdateContentRegionBounds(object sender, EventArgs e) {
            UpdateScrollbar();

            //if (this.Children.Any()) {
            //    if (Math.Max(this.ContentRegion.Width, this.Children.Max(c => c.Right)) != this.ContentRenderCache.Width) {
            //        OnPropertyChanged(nameof(this.ContentRegion));

            //        ContentRenderCache.Dispose();
            //        ContentRenderCache = null;

            //        this.ContentResized?.Invoke(this, EventArgs.Empty);
            //    }

            //    contentBounds.Width  = 
            //    contentBounds.Height = Math.Max(this.ContentRegion.Height, this.Children.Max(c => c.Bottom));
            //}
        }


        private Rectangle _headerRegion;

        private void UpdateRegions() {
            int topOffset = !string.IsNullOrEmpty(_title) ? 36 : 0;
            int rightOffset = 0;
            int bottomOffset = 0;
            int leftOffset = 0;

            if (this.ShowBorder) {
                // If we have a title, then we don't need an margin (as the title region will be that offset)
                topOffset = Math.Max(topOffset, TOP_MARGIN);
                
                rightOffset += RIGHT_MARGIN;
                bottomOffset += BOTTOM_MARGIN;
                leftOffset += LEFT_MARGIN;
            }

            if (this.CanScroll)
                rightOffset += (this.ShowBorder ? 0 : 20);

            this.ContentRegion = new Rectangle(leftOffset,
                                               topOffset,
                                               _size.X - leftOffset - rightOffset, 
                                               _size.Y - topOffset - bottomOffset);

            _headerRegion = new Rectangle(leftOffset,
                                          0,
                                          ContentRegion.Width,
                                          ContentRegion.Top);
        }

        private readonly List<Adhesive.Binding> _scrollbarBindings = new List<Adhesive.Binding>();

        private void UpdateScrollbar() {
            /* TODO: Fix .CanScroll: currently you have to set it after you set other region changing settings for it
               to work correctly */
            if (this.CanScroll) {
                if (_panelScrollbar == null) 
                    _panelScrollbar = new Scrollbar(this);

                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                _scrollbarBindings.ForEach((bind) => bind.Disable());
                _scrollbarBindings.Clear();

                int psHOffset = this.ShowBorder ? -20 : 0;
                int psYOffset = this.ShowBorder ? 10 : 0;
                int psXOffset = this.ShowBorder ? -RIGHT_MARGIN - 2 : -20;

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Parent == this.Parent));
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Parent, () => this.Parent, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Height == this.Height));
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Height, () => this.Height, (h) => this.ContentRegion.Height - 20, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Right == this.Right - _panelScrollbar.Width));
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Right, () => this.Right, (r) => r - _panelScrollbar.Width / 2, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Top == this.Top));
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Top, () => this.Top, (t) => t + this.ContentRegion.Top + 10, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Visible == this.Visible));
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Visible, () => this.Visible, applyLeft: true));

                // Ensure scrollbar is visible
                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.ZIndex == this.ZIndex + 2));
                _scrollbarBindings.Add(Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.ZIndex, () => this.ZIndex, (z) => z + 2, applyLeft: true));
            } else {
                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                _scrollbarBindings.ForEach((bind) => bind.Disable());
                _scrollbarBindings.Clear();

                _panelScrollbar?.Dispose();
                _panelScrollbar = null;
            }
        }
        
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            var headerRect = _headerRegion;

            if (!string.IsNullOrEmpty(_title)) {
                spriteBatch.DrawOnCtrl(
                                       this,
                                       Content.GetTexture("accordion-header-standard"),
                                       headerRect
                                      );

                spriteBatch.DrawStringOnCtrl(this,
                                         _title,
                                         Content.DefaultFont16,
                                         headerRect.OffsetBy(10, 0),
                                         Color.White);
            }

            headerRect.Inflate(-10, 0);

            if (this.ShowBorder) {
                // Lightly tint the background of the panel
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, ContentRegion, Color.Black * 0.1f);

                // Top left accent
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("1002144"),
                                 new Rectangle(ContentRegion.Left - 6,
                                               headerRect.Bottom - 12,
                                               Math.Min(ContentRegion.Width, 256),
                                               64),
                                 null,
                                 Color.White,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.FlipHorizontally);

                // Bottom right accent
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("1002142"),
                                 new Rectangle(ContentRegion.Right - 249,
                                               ContentRegion.Bottom - 53,
                                               Math.Min(ContentRegion.Width, 256),
                                               64),
                                 null,
                                 Color.White,
                                 0,
                                 Vector2.Zero);

                // Left side accent
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("605025"),
                                 new Rectangle(ContentRegion.Left - 8,
                                               ContentRegion.Top,
                                               16,
                                               ContentRegion.Height),
                                 null,
                                 Color.Black,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.FlipVertically);
            }

            // Right side accent (if scrollbar isn't visible)
            if (this.CanScroll && !_panelScrollbar.Visible) {
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("scrollbar-track"),
                                 new Rectangle(ContentRegion.Right - 2,
                                               ContentRegion.Top,
                                               Content.GetTexture("scrollbar-track").Width,
                                               ContentRegion.Height),
                                 Color.Black);
            }
        }

        protected override void DisposeControl() {
            _panelScrollbar?.Dispose();

            base.DisposeControl();
        }

    }
}
