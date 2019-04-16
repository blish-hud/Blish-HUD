using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class Panel:Container {

        // Used when border is enabled
        public const int TOP_MARGIN    = 0;
        public const int RIGHT_MARGIN  = 5;
        public const int BOTTOM_MARGIN = 10;
        public const int LEFT_MARGIN   = 8;

        private bool _canScroll = false;
        public bool CanScroll {
            get => _canScroll;
            set {
                if (_canScroll == value) return;

                _canScroll = value;

                UpdateRegions();
                UpdateScrollbar();

                OnPropertyChanged(nameof(this.CanScroll), true);
            }
        }
        
        private string _title;
        public string Title {
            get => _title;
            set {
                if (_title == value) return;

                _title = value;
                OnPropertyChanged();

                UpdateRegions();
            }
        }

        private bool _showBorder;
        public bool ShowBorder {
            get => _showBorder;
            set {
                if (_showBorder == value) return;

                _showBorder = value;
                OnPropertyChanged();

                UpdateRegions();
            }
        }

        private Scrollbar _panelScrollbar;

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
            int topOffset = !string.IsNullOrEmpty(this.Title) ? 36 : 0;
            int rightOffset = 0;
            int bottomOffset = 0;
            int leftOffset = 0;

            if (this.ShowBorder) {
                // If we have a title, then we don't need an offset (as the title region will be that offset)
                topOffset = Math.Max(topOffset, TOP_MARGIN);
                
                rightOffset += RIGHT_MARGIN;
                bottomOffset += BOTTOM_MARGIN;
                leftOffset += LEFT_MARGIN;
            }

            if (this.CanScroll)
                rightOffset += (this.ShowBorder ? 0 : 20);

            this.ContentRegion = new Rectangle(
                                               leftOffset,
                                               topOffset,
                                               this.Width - leftOffset - rightOffset, 
                                               this.Height - topOffset - bottomOffset
                                               );

            _headerRegion = new Rectangle(
                                          leftOffset,
                                          0,
                                          this.ContentRegion.Width,
                                          this.ContentRegion.Top
                                         );
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
                _scrollbarBindings.Add(
                                       Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Parent, () => this.Parent, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Height == this.Height));
                _scrollbarBindings.Add(
                                       Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Height, () => this.Height, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Right == this.Right - _panelScrollbar.Width));
                _scrollbarBindings.Add(
                                        Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Right, () => this.Right, (r) => r - _panelScrollbar.Width, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Top == this.Top));
                _scrollbarBindings.Add(
                                        Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Top, () => this.Top, applyLeft: true));

                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.Visible == this.Visible));
                _scrollbarBindings.Add(
                                        Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.Visible, () => this.Visible, applyLeft: true));

                // Ensure scrollbar is visible
                //_scrollbarBindings.Add(Binding.Create(() => _panelScrollbar.ZIndex == this.ZIndex + 2));
                _scrollbarBindings.Add(
                                       Adhesive.Binding.CreateOneWayBinding(() => _panelScrollbar.ZIndex, () => this.ZIndex, (z) => z + 2, applyLeft: true));
            } else {
                // TODO: Switch to breaking these bindings once it is supported in Adhesive
                _scrollbarBindings.ForEach((bind) => bind.Disable());
                _scrollbarBindings.Clear();

                _panelScrollbar?.Dispose();
                _panelScrollbar = null;
            }
        }


        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            var headerRect = _headerRegion;

            if (!string.IsNullOrEmpty(this.Title)) {
                spriteBatch.Draw(
                                 Content.GetTexture("accordion-header-standard"),
                                 headerRect,
                                 Color.White
                                );

                Utils.DrawUtil.DrawAlignedText(
                                               spriteBatch,
                                               Content.GetFont(
                                                               ContentService.FontFace.Menomonia,
                                                               ContentService.FontSize.Size16,
                                                               ContentService.FontStyle.Regular
                                                              ),
                                               this.Title,
                                               headerRect.OffsetBy(10, 0),
                                               Color.White,
                                               DrawUtil.HorizontalAlignment.Left,
                                               DrawUtil.VerticalAlignment.Middle
                                              );
            }

            headerRect.Inflate(-10, 0);

            if (this.ShowBorder) {
                // Lightly tint the background of the panel
                spriteBatch.Draw(ContentService.Textures.Pixel, this.ContentRegion, Color.Black * 0.1f);

                // Top left accent
                spriteBatch.Draw(
                                 Content.GetTexture("1002144"),
                                 new Rectangle(this.ContentRegion.Left - 6, headerRect.Bottom - 12, Math.Min(this.ContentRegion.Width, 256), 64),
                                 null,
                                 Color.White,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.FlipHorizontally,
                                 0
                                );

                // Bottom right accent
                spriteBatch.Draw(
                                 Content.GetTexture("1002142"),
                                 new Rectangle(this.ContentRegion.Right - 249, this.ContentRegion.Bottom - 53, Math.Min(this.ContentRegion.Width, 256), 64),
                                 null,
                                 Color.White,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.None,
                                 0
                                );

                // Left side accent
                spriteBatch.Draw(
                                 Content.GetTexture("605025"),
                                 new Rectangle(this.ContentRegion.Left - 8, this.ContentRegion.Top, 16, this.ContentRegion.Height),
                                 null,
                                 Color.Black,
                                 0,
                                 Vector2.Zero,
                                 SpriteEffects.FlipVertically,
                                 0
                                );
            }

            // Right side accent (if scrollbar isn't visible)
            if (this.CanScroll && !_panelScrollbar.Visible) {
                spriteBatch.Draw(
                                 Content.GetTexture("scrollbar-track"),
                                 new Rectangle(this.ContentRegion.Right - 2, this.ContentRegion.Top, 4, this.ContentRegion.Height),
                                 Color.Black
                                );
            }
        }

        protected override void Dispose(bool disposing) {
            _panelScrollbar?.Dispose();

            base.Dispose(disposing);
        }

    }
}
