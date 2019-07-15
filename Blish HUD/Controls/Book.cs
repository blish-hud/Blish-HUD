using Blish_HUD.Input;
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
    public class Book : BasicWindow
    {
        // TODO: Maybe add gw2's book sounds (opens, turn page)
        // TODO: Title background texture from the original.
        private readonly BitmapFont TitleFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size32, ContentService.FontStyle.Regular);
        private readonly Texture2D TurnPageSprite;

        private static int RIGHT_PADDING = 150;
        private static int TOP_PADDING = 100;
        private static int SHEET_OFFSET = 20;

        private bool MouseOverTurnPageLeft;
        private bool MouseOverTurnPageRight;

        private List<Page> Pages = new List<Page>();
        /// <summary>
        /// The currently open page of this book.
        /// </summary>
        public Page CurrentPage { get; private set; }
        /// <summary>
        /// Creates a panel that should act as Parent for Page controls to create a book UI.
        /// </summary>
        /// <param name="scale">Scale size to keep the sheet's aspect ratio.</param>
        public Book(int scale = 1) : base(
            GameService.Content.GetTexture("1909321").Duplicate().GetRegion(0, 0, 680, 800),
            new Vector2(35, 25),
            new Rectangle(35, 25, 625, 800),
            new Thickness(0, 0, 0, 35),
            40,
            true)
        {
            HideTitle = true;
            TurnPageSprite = TurnPageSprite ?? GameService.Content.GetTexture("1909317");
            
            OnResized(null);
        }
        protected override void OnResized(ResizedEventArgs e)
        {
            ContentRegion = new Rectangle(0, 40, this.Width, this.Height - 40);
            if (Pages == null || Pages.Count <= 0) return;

            foreach (Page page in this.Pages)
            {
                if (page == null) continue;
                page.Size = PointExtensions.ResizeKeepAspect(page.Size, ContentRegion.Width - RIGHT_PADDING, ContentRegion.Height - TOP_PADDING, true);
                page.Location = new Point((ContentRegion.Width - page.Size.X) / 2, (ContentRegion.Height - page.Size.Y) / 2 + SHEET_OFFSET);
            }

            base.OnResized(e);
        }
        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (e.ChangedChild is Page && !Pages.Any(x => x.Equals((Page)e.ChangedChild)))
            {
                Page page = (Page)e.ChangedChild;
                page.Size = PointExtensions.ResizeKeepAspect(page.Size, ContentRegion.Width - RIGHT_PADDING, ContentRegion.Height - TOP_PADDING, true);
                page.Location = new Point((ContentRegion.Width - page.Size.X) / 2, (ContentRegion.Height - page.Size.Y) / 2 + SHEET_OFFSET);
                page.SetPageNumber(this, Pages.Count + 1);
                Pages.Add(page);

                if (Pages.Count == 1) CurrentPage = page;
                if (page != CurrentPage) page.Hide();
            }

            base.OnChildAdded(e);
        }
        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = this.RelativeMousePosition;

            Rectangle leftButtonBounds = new Rectangle(15, (ContentRegion.Height - TurnPageSprite.Bounds.Height) / 2 + SHEET_OFFSET, TurnPageSprite.Bounds.Width, TurnPageSprite.Bounds.Height);
            Rectangle rightButtonBounds = new Rectangle(ContentRegion.Width - TurnPageSprite.Bounds.Width - 15, (ContentRegion.Height - TurnPageSprite.Bounds.Height) / 2 + SHEET_OFFSET, TurnPageSprite.Bounds.Width, TurnPageSprite.Bounds.Height);

            this.MouseOverTurnPageLeft = leftButtonBounds.Contains(relPos);
            this.MouseOverTurnPageRight = rightButtonBounds.Contains(relPos);

            base.OnMouseMoved(e);
        }
        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (this.MouseOverTurnPageLeft)
            {
                TurnPage(Pages.IndexOf(CurrentPage) - 1);
            }
            else if (this.MouseOverTurnPageRight)
            {
                TurnPage(Pages.IndexOf(CurrentPage) + 1);
            }

            base.OnLeftMouseButtonPressed(e);
        }
        private void TurnPage(int index)
        {
            if (index < Pages.Count && index >= 0)
            {
                CurrentPage = Pages[index];

                foreach (Page other in Pages)
                {
                    other.Visible = other == CurrentPage;
                }
            }
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            Point titleSize = (Point)TitleFont.MeasureString(this.Title);
            Rectangle titleDest = new Rectangle((ContentRegion.Width - titleSize.X) / 2, ContentRegion.Top + (TOP_PADDING - titleSize.Y) / 2, titleSize.X, titleSize.Y);
            spriteBatch.DrawStringOnCtrl(this, Title, TitleFont, titleDest, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Top);

            Rectangle leftButtonBounds = new Rectangle(15, (ContentRegion.Height - TurnPageSprite.Bounds.Height) / 2 + SHEET_OFFSET, TurnPageSprite.Bounds.Width, TurnPageSprite.Bounds.Height);
            Rectangle rightButtonBounds = new Rectangle(ContentRegion.Width - TurnPageSprite.Bounds.Width - 15, (ContentRegion.Height - TurnPageSprite.Bounds.Height) / 2 + SHEET_OFFSET, TurnPageSprite.Bounds.Width, TurnPageSprite.Bounds.Height);

            if (!MouseOverTurnPageLeft)
            {
                spriteBatch.DrawOnCtrl(this, TurnPageSprite, leftButtonBounds, TurnPageSprite.Bounds, new Color(155, 155, 155, 150), 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, TurnPageSprite, leftButtonBounds, TurnPageSprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            }

            if (!MouseOverTurnPageRight)
            {
                spriteBatch.DrawOnCtrl(this, TurnPageSprite, rightButtonBounds, TurnPageSprite.Bounds, new Color(155, 155, 155, 155));
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, TurnPageSprite, rightButtonBounds, TurnPageSprite.Bounds, Color.White);
            }
        }
    }
}
