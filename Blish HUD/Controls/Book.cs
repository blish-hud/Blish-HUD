using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls
{
    public class Book : WindowBase
    {
        private const int RIGHT_PADDING = 50;
        private const int TOP_PADDING = 100;

        private readonly Texture2D BookSheetSprite;
        private readonly Texture2D PanelBackgroundSprite;

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

        public List<string> Contents { get; private set; }

        /// <summary>
        /// Creates a control similar to the Tyrian' sheet of paper or book control.
        /// </summary>
        /// <param name="location">Location to draw the control.</param>
        /// <param name="scale">The scale by which to scale the control.</param>
        public Book() : base()
        {
            Contents = new List<string>();

            BookSheetSprite = BookSheetSprite ?? Content.GetTexture("1909316");
            PanelBackgroundSprite = PanelBackgroundSprite ?? Content.GetTexture("1909321").Duplicate().GetRegion(0, 0, 660, 800);
            Rectangle windowBackgroundBounds = new Rectangle(0, 0, 625, 775);

            this.ContentRegion = windowBackgroundBounds;

            // Construct background panel.
            ConstructWindow(PanelBackgroundSprite, 
                new Vector2(0, 0),
                windowBackgroundBounds,
                new Thickness(0, 0, 0, 35),
                40,
                true
            );

            sheetSize = PointExtensions.ResizeKeepAspect(BookSheetSprite.Bounds.Size, this.Width - RIGHT_PADDING, this.Height - TOP_PADDING, true);
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            Point center = new Point((this.ContentRegion.Width - sheetSize.X) / 2, (this.ContentRegion.Height - sheetSize.Y) / 2);
            spriteBatch.DrawOnCtrl(this, BookSheetSprite, new Rectangle(center, sheetSize), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None);

            if (Contents.Count > 0)
            {
                spriteBatch.DrawStringOnCtrl(this, Contents[currentPage], Content.DefaultFont14, BookSheetSprite.Bounds, Color.White, true, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }
    }
}
