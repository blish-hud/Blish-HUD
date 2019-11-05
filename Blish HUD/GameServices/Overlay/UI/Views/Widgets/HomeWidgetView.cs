using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Overlay.UI.Views.Widgets {

    public interface IHomeWidgetView : IView {



    }

    public abstract class HomeWidgetView<TPresenter> : View<TPresenter>, IHomeWidgetView where TPresenter : class, IPresenter {

        #region Load Static

        private static readonly Texture2D _textureWidgetBackground;

        static HomeWidgetView() {
            _textureWidgetBackground = GameService.Content.GetTexture(@"common\backgrounds\155209");
        }

        #endregion

        private Label _widgetTitleText;

        public string Title {
            get => _widgetTitleText.Text;
            set => _widgetTitleText.Text = value;
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            buildPanel.Size              = new Point(450, 225);
            buildPanel.BackgroundTexture = _textureWidgetBackground;

            _widgetTitleText = new Label() {
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Left           = 20,
                TextColor      = Color.White,
                StrokeText     = true,
                Font           = GameService.Content.DefaultFont16,
                Parent         = buildPanel,
            };
        }

    }
}
