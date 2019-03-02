using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {

    public class ChildChangedEventArgs:CancelEventArgs {
        public Control ChangedChild { get; }
        public bool Added { get; }
        public List<Control> ResultingChildren { get; }

        public ChildChangedEventArgs(Container sender, Control changedChild, bool adding) {
            this.ChangedChild = changedChild;
            this.Added = adding;

            this.ResultingChildren = new List<Control>(sender.Children.ToList());

            if (adding)
                this.ResultingChildren.Add(changedChild);
            else
                this.ResultingChildren.Remove(changedChild);
        }
    }

    // TODO: Ensure that container objects, when disposed, first dispose of their children
    public abstract class Container : Control, IEnumerable<Control> { // : Control where T : Control {

        public event EventHandler<ChildChangedEventArgs> ChildAdded;
        public event EventHandler<ChildChangedEventArgs> ChildRemoved;


        public class RegionChangedEventArgs:EventArgs {
            public Rectangle PreviousRegion { get; }
            public Rectangle CurrentRegion  { get; }

            public RegionChangedEventArgs(Rectangle previousRegion, Rectangle currentRegion) {
                this.PreviousRegion = previousRegion;
                this.CurrentRegion  = currentRegion;
            }
        }
        public event EventHandler<RegionChangedEventArgs> ContentResized;

        protected virtual void OnChildAdded(ChildChangedEventArgs e) {
            this.ChildAdded?.Invoke(this, e);
        }

        protected virtual void OnChildRemoved(ChildChangedEventArgs e) {
            this.ChildRemoved?.Invoke(this, e);
        }

        protected virtual void OnContentResized(RegionChangedEventArgs e) {
            this.ContentResized?.Invoke(this, e);
        }

        private List<Control> _children;
        [Newtonsoft.Json.JsonIgnore]
        public IReadOnlyCollection<Control> Children => _children.AsReadOnly();

        protected Container() {
            _children = new List<Control>();
        }

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);
            
            /* ContentRegion defaults to match our control size until one is manually set,
               so we do squeeze in OnPropertyChanged for it if the control hasn't had a
               ContentRegion specified and then resizes */
            if (!_contentRegion.HasValue)
                OnPropertyChanged(nameof(this.ContentRegion));
        }

        public void AddChild(Control child) {
            if (this.Children.Contains(child)) return;

            var evRes = new ChildChangedEventArgs(this, child, true);
            OnChildAdded(evRes);

            if (!evRes.Cancel) {
                _children.Add(child);
                Invalidate();
            }
        }

        public void RemoveChild(Control child) {
            if (!this.Children.Contains(child)) return;

            var evRes = new ChildChangedEventArgs(this, child, false);
            OnChildRemoved(evRes);

            if (!evRes.Cancel) {
                _children.Remove(child);
                Invalidate();
            }
        }

        private Rectangle? _contentRegion;
        public Rectangle ContentRegion {
            get => (Rectangle) (_contentRegion ?? new Rectangle(Point.Zero, this.Size));
            protected set {
                if (_contentRegion == value) return;

                var _previousRegion = this.ContentRegion;

                _contentRegion = value;
                OnPropertyChanged();

                OnContentResized(new RegionChangedEventArgs(_previousRegion, this.ContentRegion));
            }
        }

        private int _verticalScrollOffset;
        public int VerticalScrollOffset {
            get => _verticalScrollOffset;
            set {
                if (_verticalScrollOffset == value) return;

                _verticalScrollOffset = value;
                OnPropertyChanged();
            }
        }

        public RenderTarget2D ContentRenderCache;

        public List<Control> GetDescendants() {
            var allDescendants = new List<Control>(this.Children);

            foreach (var child in this.Children) {
                if (!(child is Container container)) continue;

                allDescendants.AddRange(container.GetDescendants());
            }

            return allDescendants;
        }

        public abstract void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds);

        public override bool TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
            List<Control> zSortedChildren = this.Children.OrderByDescending(i => i.ZIndex).ToList();

            if (mouseEventType == MouseEventType.MouseMoved) base.TriggerMouseInput(mouseEventType, ms);

            bool mouseCheck = false;

            foreach (var childControl in zSortedChildren) {
                if (childControl.AbsoluteBounds.Contains(ms.Position) && childControl.Visible && childControl.TriggerMouseInput(mouseEventType, ms)) {
                    mouseCheck = true;
                    break;
                }
            }

            if (mouseEventType == MouseEventType.MouseMoved) {
                return mouseCheck | base.TriggerMouseInput(mouseEventType, ms);
            }
            
            return mouseCheck || base.TriggerMouseInput(mouseEventType, ms);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            foreach (var childControl in this.Children.ToList()) {
                if (childControl.Visible)
                    childControl.Update(gameTime);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // NOOP
        }

        private Rectangle LastContentBounds = Rectangle.Empty;

        private void PaintContent(GraphicsDevice graphicsDevice, IEnumerable<Control> sortedChildren) {
            var contentBounds = this.ContentRegion;

            if (this.Children.Any()) {
                contentBounds.Width  = Math.Max(this.ContentRegion.Width,  this.Children.Max(c => c.Right));
                contentBounds.Height = Math.Max(this.ContentRegion.Height, this.Children.Max(c => c.Bottom));
            }

            if (ContentRenderCache != null) {
                if (ContentRenderCache.Width != contentBounds.Width || ContentRenderCache.Height != contentBounds.Height) {
                    ContentRenderCache?.Dispose();
                    ContentRenderCache = null;
                }
            }

            if (LastContentBounds != contentBounds) OnContentResized(new RegionChangedEventArgs(LastContentBounds, contentBounds ));

            LastContentBounds = contentBounds;

            if (ContentRenderCache == null && contentBounds.Width * contentBounds.Height > 0) {
                ContentRenderCache = new RenderTarget2D(
                    graphicsDevice,
                    contentBounds.Width,
                    contentBounds.Height,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None,
                    graphicsDevice.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PreserveContents
                );
            }

            var contentSpritebatch = new SpriteBatch(graphicsDevice);
            graphicsDevice.SetRenderTarget(ContentRenderCache);

            graphicsDevice.Clear(Color.Transparent);

            // Paint children
            foreach (var childControl in sortedChildren) {
                if (childControl.Visible) {
                    contentSpritebatch.Begin(
                                             SpriteSortMode.Immediate,
                                             childControl.BlendState,
                                             null,
                                             null,
                                             null,
                                             childControl.DrawEffect
                                            );

                    var childRender = childControl.GetRender();

                    if (childRender != null)
                        contentSpritebatch.Draw(
                                            childControl.GetRender(),
                                            childControl.OuterBounds.OffsetBy(this.Padding),
                                            Color.White * childControl.Opacity
                                           );
                    else 
                        Console.WriteLine($"Child control {childControl.GetType().FullName} did not provide anything to render.");

                    contentSpritebatch.End();
                }
            }

            graphicsDevice.SetRenderTarget(null);
            //graphicsDevice.Clear(Color.Transparent);

            contentSpritebatch.Dispose();
        }

        public override void Draw(GraphicsDevice graphicsDevice, Rectangle bounds) {
            if (this.NeedsRedraw || RenderCache == null) {
                //if (RenderCache != null && (RenderCache.Width != this.Width || RenderCache.Height != this.Height))
                    RenderCache?.Dispose();

                //if (RenderCache == null) {
                    try {
                        RenderCache = new RenderTarget2D(
                                                         graphicsDevice, this.Width + this.Padding.X * 2, this.Height + this.Padding.Y * 2,
                                                         false,
                                                         SurfaceFormat.Color,
                                                         DepthFormat.None,
                                                         graphicsDevice.PresentationParameters.MultiSampleCount,
                                                         RenderTargetUsage.PreserveContents
                                                        );
                    } catch (ArgumentOutOfRangeException aoorEx) {
                        // TODO: Use debug service to write to log that we are trying to create a render target that is too small
                        Console.WriteLine($"{this.GetType().FullName} attempted to render at an invalid size: {this.Width}x{this.Height}");

                        return;
                    } catch (Exception generalEx) {
                        // TODO: Use debug service to write to log that we had an unexpected error
                        Console.WriteLine($"{this.GetType().FullName} had an unexpected error when attempting to render:");
                        Console.WriteLine(generalEx.Message);

                        return;
                    }
                //}

                // Allow children to have a chance to render if they need to
                List<Control> zSortedChildren = this.Children.OrderBy(i => i.ZIndex).ToList();

                foreach (var childControl in zSortedChildren) {
                    if (childControl.Visible)
                        childControl.Draw(graphicsDevice, new Rectangle(childControl.Padding, childControl.Size));
                }

                PaintContent(graphicsDevice, zSortedChildren);

                var ctrlSpritebatch = new SpriteBatch(graphicsDevice);
                graphicsDevice.SetRenderTarget(RenderCache);

                graphicsDevice.Clear(Color.Transparent);

                ctrlSpritebatch.Begin();

                // Paint container background
                PaintContainer(ctrlSpritebatch, bounds);
                //Paint(ctrlSpritebatch, Bounds);

                // TODO: Only if debugging
                //ctrlSpritebatch.Draw(ContentService.Textures.Pixel, this.ContentRegion, Color.Blue);
                if (ContentRenderCache != null)
                    ctrlSpritebatch.Draw(ContentRenderCache, this.ContentRegion, new Rectangle(0, this.VerticalScrollOffset, this.ContentRegion.Width, this.ContentRegion.Height), Color.White);

                ctrlSpritebatch.End();
                graphicsDevice.SetRenderTarget(null);

                ctrlSpritebatch.Dispose();
                graphicsDevice.Clear(Color.Transparent);
            }

            this.NeedsRedraw = false;
        }

        public IEnumerator<Control> GetEnumerator() {
            return this.Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.Children.GetEnumerator();
        }

    }
}
