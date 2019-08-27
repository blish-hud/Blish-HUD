using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class ViewContainer : Panel {

        private Panel _buildPanel;
        private IView _currentView;

        private ViewState _viewState = ViewState.None;

        private string _loadingMessage;

        public ViewState ViewState => _viewState;

        public void Show(IView newView) {
            _currentView?.DoUnload();
            _buildPanel?.Dispose();

            _viewState = ViewState.Loading;

            _currentView = newView;

            var progressIndicator = new Progress<string>((progressReport) => { _loadingMessage = progressReport; });

            newView.Loaded += BuildView;
            newView.DoLoad(progressIndicator).ContinueWith(BuildView);

            base.Show();
        }

        private void BuildView(object sender, EventArgs e) {
            _currentView.Loaded -= BuildView;

            _viewState = ViewState.Loaded;
        }

        private void BuildView(Task<bool> loadResult) {
            if (loadResult.Result) {
                _buildPanel = new Panel() {
                    Size = this.ContentRegion.Size
                };

                _currentView.DoBuild(_buildPanel);
                _buildPanel.Parent = this;
            }
        }

        /// <inheritdoc />
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_viewState == ViewState.Loading) {
                spriteBatch.DrawStringOnCtrl(this, _loadingMessage ?? "", Content.DefaultFont14, bounds, Color.White, false, true, 1, HorizontalAlignment.Center);
            }
        }

    }
}
