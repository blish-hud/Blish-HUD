using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Blish_HUD.Controls
{
    public class Book : Control
    {
        private static BitmapFont ContentFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private static BitmapFont PageNumberFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
        private const int RIGHT_PADDING = 50;
        private const int TOP_PADDING = 100;
        private const int FIX_WORDCLIPPING_WIDTH = 20;
        private readonly Texture2D BookSheetSprite;
        private Point sheetSize;

        private int currentPage = 0;
        public int CurrentPage
        {
            get => currentPage;
            set {
                if (value == currentPage) return;
                SetProperty(ref currentPage, value, true);
            }
        }

        public List<string> Contents;

        /// <summary>
        /// Creates a control similar to the Tyrian' sheet of paper or book control.
        /// </summary>
        /// <param name="scale">Scale size to keep the sheet's aspect ratio.</param>
        public Book(int scale = 1)
        {
            this.Size = new Point(625 * scale, 775 * scale);
            BookSheetSprite = BookSheetSprite ?? Content.GetTexture("1909316");
            sheetSize = PointExtensions.ResizeKeepAspect(BookSheetSprite.Bounds.Size, this.Width - RIGHT_PADDING, this.Height - TOP_PADDING, true);
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Point center = new Point((this.Size.X - sheetSize.X) / 2, (this.Size.Y - sheetSize.Y) / 2);
            spriteBatch.DrawOnCtrl(this, BookSheetSprite, new Rectangle(center, sheetSize), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);

            if (Contents != null && currentPage < Contents.Count)
            {
                Rectangle contentArea = new Rectangle(new Point(center.X + 40, center.Y + 40), new Point(sheetSize.X - 80 - FIX_WORDCLIPPING_WIDTH, sheetSize.Y - 80));
                spriteBatch.DrawStringOnCtrl(this, Contents[currentPage], ContentFont, contentArea, Color.Black, true, HorizontalAlignment.Left, VerticalAlignment.Top);
                string pageNumber = (currentPage + 1).ToString();
                Point pageNumberSize = (Point)PageNumberFont.MeasureString(pageNumber);
                Point pageNumberCenter = new Point((this.Size.X - pageNumberSize.X) / 2, sheetSize.Y - (pageNumberSize.Y / 2));
                spriteBatch.DrawStringOnCtrl(this, pageNumber, PageNumberFont, new Rectangle(pageNumberCenter, pageNumberSize), Color.Black, true, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }
    }
}
