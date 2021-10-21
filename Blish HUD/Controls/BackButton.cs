using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class BackButton : Control {

        private const int BACKBUTTON_WIDTH  = 280;
        private const int BACKBUTTON_HEIGHT = 54;

        private const int BACKBUTTON_ICON_PADDING = 9;
        private const int BACKBUTTON_ICON_SIZE = 36;

        #region Load Static

        private static readonly Texture2D _textureBackButton = Content.GetTexture("784268");

        #endregion

        protected string _text = "button";
        /// <summary>
        /// The primary text of the back button.  Format is "Text: NavTitle"
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        /// <summary>
        /// The secondary path of the back button.  Format is "Text: NavTitle"
        /// </summary>
        protected string _navTitle;
        public string NavTitle {
            get => _navTitle;
            set => SetProperty(ref _navTitle, value);
        }

        private readonly WindowBase _window;

        public BackButton(WindowBase window) : base() {
            this.Size = new Point(BACKBUTTON_WIDTH, BACKBUTTON_HEIGHT);

            this.EffectBehind = new Effects.ScrollingHighlightEffect(this);

            _window = window;
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            _window.NavigateBack();
        }

        #region Calculated Layout

        private Rectangle _layoutButtonIconBounds;
        private Rectangle _layoutTextBounds;

        #endregion

        public override void RecalculateLayout() {
            _layoutButtonIconBounds = new Rectangle(BACKBUTTON_ICON_PADDING, BACKBUTTON_ICON_PADDING, BACKBUTTON_ICON_SIZE, BACKBUTTON_ICON_SIZE);
            _layoutTextBounds = new Rectangle(BACKBUTTON_HEIGHT, 0, _size.X - BACKBUTTON_HEIGHT, _size.Y);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw back button
            spriteBatch.DrawOnCtrl(this, _textureBackButton, _layoutButtonIconBounds);
            
            // Draw the full tab path (Tab: Subtab)
            spriteBatch.DrawStringOnCtrl(this, $"{_text}: {_navTitle}",
                                         Content.DefaultFont16,
                                         _layoutTextBounds,
                                         Color.White * 0.8f);

            // Draw just the tab name
            spriteBatch.DrawStringOnCtrl(this, $"{_text}:",
                                         Content.DefaultFont16,
                                         _layoutTextBounds,
                                         Color.White * 0.8f);
        }

    }
}
