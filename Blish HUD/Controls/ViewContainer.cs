using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    /// <summary>
    /// Acts as a container to build and house the lifecycle of any provided <see cref="IView"/>.
    /// </summary>
    public class ViewContainer : Panel {

        private const float FADE_DURATION = 0.35f;

        /// <summary>
        /// The current state of the view.
        /// </summary>
        public ViewState ViewState { get; private set; } = ViewState.None;

        private bool _fadeView = false;

        /// <summary>
        /// If views should be faded in as they do in Guild Wars 2.
        /// Default is <c>false</c> which will show the built view immediately.
        /// </summary>
        public bool FadeView {
            get => _fadeView;
            set => SetProperty(ref _fadeView, value);
        }

        /// <summary>
        /// The <see cref="IView"/> this container is currently displaying.
        /// </summary>
        public IView CurrentView { get; private set; }

        private Tween _fadeInAnimation;

        private string _loadingMessage;

        /// <summary>
        /// Shows the provided view.
        /// </summary>
        public void Show(IView newView) {
            Clear();

            ViewState = ViewState.Loading;

            this.CurrentView = newView;

            var progressIndicator = new Progress<string>((progressReport) => { _loadingMessage = progressReport; });

            newView.Loaded += BuildView;
            newView.DoLoad(progressIndicator).ContinueWith(BuildView);

            if (_fadeView) {
                _fadeInAnimation = GameService.Animation.Tweener.Tween(this, new {Opacity = 1f}, FADE_DURATION);
            }

            base.Show();
        }

        /// <summary>
        /// Clear the view from this container.
        /// </summary>
        public void Clear() {
            this.CurrentView?.DoUnload();

            // Reset panel defaults
            this.BackgroundColor   = Color.Transparent;
            this.BackgroundTexture = null;
            this.ClipsBounds       = true;

            // Potentially prepare for next fade-in
            _fadeInAnimation?.Cancel();
            _fadeInAnimation = null;
            _opacity         = _fadeView ? 0f : 1f;

            this.ClearChildren();
        }

        private void BuildView(object sender, EventArgs e) {
            this.CurrentView.Loaded -= BuildView;

            ViewState = ViewState.Loaded;
        }

        private void BuildView(Task<bool> loadResult) {
            if (loadResult.Result) {
                this.CurrentView.DoBuild(this);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (ViewState == ViewState.Loading) {
                spriteBatch.DrawStringOnCtrl(this, _loadingMessage ?? "", Content.DefaultFont14, this.ContentRegion, Color.White, false, true, 1, HorizontalAlignment.Center);
            }
        }

        protected override void DisposeControl() {
            this.Clear();

            base.DisposeControl();
        }

    }
}
