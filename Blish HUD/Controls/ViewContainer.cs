using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class ViewContainer : Panel {

        private const float FADE_DURATION = 0.35f;

        private Panel _buildPanel;
        private IView _currentView;

        private ViewState _viewState = ViewState.None;

        private string _loadingMessage;

        public ViewState ViewState => _viewState;

        private Tween _fadeInAnimation;

        public void Show(IView newView) {
            this.Clear();

            _viewState = ViewState.Loading;

            _currentView = newView;

            var progressIndicator = new Progress<string>((progressReport) => { _loadingMessage = progressReport; });

            newView.Loaded += BuildView;
            newView.DoLoad(progressIndicator).ContinueWith(BuildView);

            _fadeInAnimation?.CancelAndComplete();
            _opacity         = 0f;
            _fadeInAnimation = GameService.Animation.Tweener.Tween(this, new { Opacity = 1f }, FADE_DURATION);

            base.Show();
        }

        /// <summary>
        /// Clear the view from this container.
        /// </summary>
        public void Clear() {
            _currentView?.DoUnload();

            // Reset panel defaults
            this.BackgroundColor   = Color.Transparent;
            this.BackgroundTexture = null;
            this.ClipsBounds       = true;

            this.ClearChildren();
        }

        private void BuildView(object sender, EventArgs e) {
            _currentView.Loaded -= BuildView;

            _viewState = ViewState.Loaded;
        }

        private void BuildView(Task<bool> loadResult) {
            if (loadResult.Result) {
                _currentView.DoBuild(this);
            }
        }

        /// <inheritdoc />
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (_viewState == ViewState.Loading) {
                spriteBatch.DrawStringOnCtrl(this, _loadingMessage ?? "", Content.DefaultFont14, this.ContentRegion, Color.White, false, true, 1, HorizontalAlignment.Center);
            }
        }

        /// <inheritdoc />
        protected override void DisposeControl() {
            this.Clear();

            base.DisposeControl();
        }

    }
}
