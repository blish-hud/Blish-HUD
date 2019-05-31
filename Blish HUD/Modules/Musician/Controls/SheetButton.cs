using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Musician.Notation.Persistance;
namespace Blish_HUD.Modules.Musician.Controls {

    // TODO: Show "Edit" button when music sheet creator correlates to account name from ApiService. Navigates to composer.
    public class SheetButton : DetailsButton {

        private const int SHEETBUTTON_WIDTH = 327;
        private const int SHEETBUTTON_HEIGHT = 100;

        private const int USER_WIDTH = 75;
        private const int BOTTOMSECTION_HEIGHT = 35;

        public string Artist { get; set; }
        public string Title { get; set; }
        public string User { get; set; }
        private static Texture2D BeatmaniaSprite;
        private static Texture2D AutoplaySprite;
        private static Texture2D BackgroundSprite;
        private static Texture2D DividerSprite;
        private static Texture2D IconBoxSprite;
        private Texture2D PreviewSprite;
        private bool _isPreviewing;
        public bool IsPreviewing
        {
            get => _isPreviewing;
            set
            {
                if (value == _isPreviewing) return;
                _isPreviewing = value;
                Invalidate();
            }
        }
        private Texture2D _icon;
        public Texture2D Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;

                _icon = value;
                OnPropertyChanged();
            }
        }
        private DetailsIconSize IconSize;
        private RawMusicSheet _musicSheet;
        public RawMusicSheet MusicSheet
        {
            get => _musicSheet;
            set
            {
                if (_musicSheet == value) return;

                _musicSheet = value;
                OnPropertyChanged();
            }
        }
        public SheetButton()
        {

            BeatmaniaSprite = BeatmaniaSprite ?? Content.GetTexture("beatmania");
            AutoplaySprite = AutoplaySprite ?? Content.GetTexture("autoplay");
            BackgroundSprite = BackgroundSprite ?? ContentService.Textures.Pixel;
            DividerSprite = DividerSprite ?? Content.GetTexture("157218");
            IconBoxSprite = IconBoxSprite ?? Content.GetTexture("605003");
            this.MouseMoved += SheetButton_MouseMoved;
            this.MouseLeft += SheetButton_MouseLeft;
            this.Size = new Point(SHEETBUTTON_WIDTH, SHEETBUTTON_HEIGHT);
        }
        private bool _mouseOverPlay = false;
        public bool MouseOverPlay
        {
            get => _mouseOverPlay;
            set
            {
                if (_mouseOverPlay == value) return;
                _mouseOverPlay = value;
                Invalidate();
            }
        }
        private bool _mouseOverEmulate = false;
        public bool MouseOverEmulate
        {
            get => _mouseOverEmulate;
            set
            {
                if (_mouseOverEmulate == value) return;
                _mouseOverEmulate = value;
                Invalidate();
            }
        }
        private bool _mouseOverPreview = false;
        public bool MouseOverPreview
        {
            get => _mouseOverPreview;
            set
            {
                if (_mouseOverPreview == value) return;
                _mouseOverPreview = value;
                Invalidate();
            }
        }
        private void SheetButton_MouseLeft(object sender, MouseEventArgs e)
        {
            this.MouseOverPlay = false;
            this.MouseOverEmulate = false;
        }
        private void SheetButton_MouseMoved(object sender, MouseEventArgs e)
        {
            var relPos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (this.MouseOver && relPos.Y > this.Height - BOTTOMSECTION_HEIGHT) {

                this.MouseOverPreview = relPos.X < (SHEETBUTTON_WIDTH - 36 + 32) && relPos.X > (SHEETBUTTON_WIDTH - 36);
                this.MouseOverPlay = relPos.X < (SHEETBUTTON_WIDTH - 73 + 32) && relPos.X > (SHEETBUTTON_WIDTH - 73);
                this.MouseOverEmulate = relPos.X < (SHEETBUTTON_WIDTH - 109 + 32) && relPos.X > (SHEETBUTTON_WIDTH - 109);
            } else {

                this.MouseOverPlay = false;
                this.MouseOverEmulate = false;
                this.MouseOverPreview = false;
            }

            if (this.MouseOverPlay)
            {

                this.BasicTooltipText = $"Practice mode (Synthesia)";

            }
            else if (this.MouseOverEmulate)
            {

                this.BasicTooltipText = $"Emulate keys (Autoplay)";

            }
            else if (this.MouseOverPreview)
            {
                this.BasicTooltipText = "Preview";
            } else {
                this.BasicTooltipText = this.Title;
            }
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            int iconSize = this.IconSize == DetailsIconSize.Large ? SHEETBUTTON_HEIGHT : SHEETBUTTON_HEIGHT - BOTTOMSECTION_HEIGHT;

            // Draw background
            spriteBatch.DrawOnCtrl(this, BackgroundSprite, bounds, Color.Black * 0.25f);

            // Draw bottom section (overlap to make background darker here)
            spriteBatch.DrawOnCtrl(this, BackgroundSprite, new Rectangle(0, bounds.Height - BOTTOMSECTION_HEIGHT, bounds.Width - BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT), Color.Black * 0.1f);

            // Draw preview icon
            if (this._mouseOverPreview)
            {
                if (this.IsPreviewing) {
                    spriteBatch.DrawOnCtrl(this, Content.GetTexture("glow_stop"), new Rectangle(SHEETBUTTON_WIDTH - 36, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
                } else {
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("glow_play"), new Rectangle(SHEETBUTTON_WIDTH - 36, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
                }
            }
            else
            {
                if (this.IsPreviewing)
                {
                    spriteBatch.DrawOnCtrl(this, Content.GetTexture("stop"), new Rectangle(SHEETBUTTON_WIDTH - 36, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, Content.GetTexture("play"), new Rectangle(SHEETBUTTON_WIDTH - 36, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
                }
            }

            // Draw beatmania icon
            if (this._mouseOverPlay)
            {
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("glow_beatmania"), new Rectangle(SHEETBUTTON_WIDTH - 73, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, BeatmaniaSprite, new Rectangle(SHEETBUTTON_WIDTH - 73, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
            }

            if (this._mouseOverEmulate)
            {
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("glow_autoplay"), new Rectangle(SHEETBUTTON_WIDTH - 109, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, AutoplaySprite, new Rectangle(SHEETBUTTON_WIDTH - 109, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
            }
            // Draw bottom section seperator
            spriteBatch.DrawOnCtrl(this, DividerSprite, new Rectangle(0, bounds.Height - 40, bounds.Width, 8), Color.White);

            // Draw instrument icon
            if (this.Icon != null)
            {
                spriteBatch.DrawOnCtrl(this, this.Icon, new Rectangle((bounds.Height - BOTTOMSECTION_HEIGHT) / 2 - 32, (bounds.Height - 35) / 2 - 32, 64, 64), Color.White);
                // Draw icon box
                if (this.IconSize == DetailsIconSize.Large)
                {
                    spriteBatch.DrawOnCtrl(this, Content.GetTexture("605003"), new Rectangle(0, 0, iconSize, iconSize), Color.White);
                }
            }
            // Wrap text
            string track = Title + @" - " + Artist;
            string wrappedText = Utils.DrawUtil.WrapText(Content.DefaultFont14, track, SHEETBUTTON_WIDTH - 40 - iconSize - 20);
            spriteBatch.DrawStringOnCtrl(this, wrappedText, Content.DefaultFont14, new Rectangle(89, 0, 216, this.Height - BOTTOMSECTION_HEIGHT), Color.White, false, true, 2, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);

            // Draw the user;
            spriteBatch.DrawStringOnCtrl(this, this.User, Content.DefaultFont14, new Rectangle(5, bounds.Height - BOTTOMSECTION_HEIGHT, USER_WIDTH, 35), Color.White, false, false, 0, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);
        }
    }
}