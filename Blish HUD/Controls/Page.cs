using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;
namespace Blish_HUD.Controls
{
    public class Page : Control
    {
        private static BitmapFont PageNumberFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
        private const int SHEET_BORDER = 40;
        private const int FIX_WORDCLIPPING_WIDTH = 30;
        private readonly Texture2D SheetSprite;

        private FontSize CurrentFontSize = ContentService.FontSize.Size24;

        private int PageNumber = 1;

        private string _text = "";
        /// <summary>
        /// The text to display on the sheet.
        /// </summary>
        public string Text {
            get => _text;
            set {
                if (value.Equals(_text)) return;
                SetProperty(ref _text, value, true);
            }
        }
        private BitmapFont _textFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        /// <summary>
        /// The font that is for the text.
        /// </summary>
        public BitmapFont TextFont
        {
            get => _textFont;
            set
            {
                if (value.Equals(_textFont)) return;
                SetProperty(ref _textFont, value, true);
            }
        }
        /// <summary>
        /// Creates a control similar to the Tyrian' sheet of paper found on the book UI.
        /// </summary>
        /// <param name="scale">Scale size to keep the sheet's aspect ratio.</param>
        public Page(int scale = 1)
        {
            Size = new Point(420 * scale, 560 * scale);
            SheetSprite = SheetSprite ?? Content.GetTexture("1909316");
        }
        /// <summary>
        /// Sets the sheet's page number. Only setable by its parent book.
        /// </summary>
        /// <param name="parent">Its parent Book control</param>
        /// <param name="number">The page number to be displayed.</param>
        public void SetPageNumber(Book parent, int number)
        {
            if (parent == this.Parent)
            {
                PageNumber = number;
            }
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, SheetSprite, bounds, SheetSprite.Bounds, Color.White, 0f, Vector2.Zero, SpriteEffects.None);

            if (_text.Length > 0)
            {
                Rectangle contentArea = new Rectangle(new Point(SHEET_BORDER, SHEET_BORDER), new Point(this.Size.X - (SHEET_BORDER * 2) - FIX_WORDCLIPPING_WIDTH, this.Size.Y - (SHEET_BORDER * 2)));
                spriteBatch.DrawStringOnCtrl(this, _text, _textFont, contentArea, Color.Black, true, HorizontalAlignment.Left, VerticalAlignment.Top);
                string pageNumber = PageNumber + "";
                Point pageNumberSize = (Point)PageNumberFont.MeasureString(pageNumber);
                Point pageNumberCenter = new Point((this.Size.X - pageNumberSize.X) / 2, this.Size.Y - pageNumberSize.Y - (SHEET_BORDER / 2));
                spriteBatch.DrawStringOnCtrl(this, pageNumber, PageNumberFont, new Rectangle(pageNumberCenter, pageNumberSize), Color.Black, false, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }
    }
}
