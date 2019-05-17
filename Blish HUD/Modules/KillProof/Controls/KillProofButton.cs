using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Blish_HUD.Controls;
namespace Blish_HUD.Modules.KillProof.Controls
{

    /// <summary>
    /// Used to show details and multiple options for a single topic. Designed to look like the Achievements details icon.
    /// </summary>
    public class KillProofButton : DetailsButton
    {
        private BitmapFont _font;
        public BitmapFont Font
        {
            get => _font;
            set
            {
                if (_font == value) return;

                _font = value;
                OnPropertyChanged();
            }
        }
        private string _bottomText = "z";
        public string BottomText
        {
            get => _bottomText;
            set
            {
                if (_bottomText == value) return;
                _bottomText = value;
            }
        }
        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;

                _title = value;
                OnPropertyChanged();
            }
        }
        public KillProofButton()
        {
            this.EVENTSUMMARY_WIDTH = 327;
            this.Size = new Point(EVENTSUMMARY_WIDTH, EVENTSUMMARY_HEIGHT);
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw background
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.25f);

            // Draw bottom section
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this.ContentRegion, Color.Black * 0.1f);

            int iconSize = this.IconSize == DetailsIconSize.Large ? EVENTSUMMARY_HEIGHT : EVENTSUMMARY_HEIGHT - BOTTOMSECTION_HEIGHT;

            // Draw bottom text
            if (Title != "")
            {
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("icon_title"), new Rectangle(EVENTSUMMARY_WIDTH - 36, bounds.Height - BOTTOMSECTION_HEIGHT + 1, 32, 32), Color.White);
            }
            else
            {
                spriteBatch.DrawStringOnCtrl(this, this.BottomText, Content.DefaultFont14, new Rectangle(iconSize + 20, iconSize - BOTTOMSECTION_HEIGHT, EVENTSUMMARY_WIDTH - 40, BOTTOMSECTION_HEIGHT), Color.White, false, true, 2);
            }
            if (this.Icon != null)
            {
                // Draw icon (the ternary in there is nasty - need to do a better way to offset this
                spriteBatch.DrawOnCtrl(this, this.Icon, new Rectangle(iconSize / 2 - 64 / 2 + (this.IconSize == DetailsIconSize.Small ? 10 : 0), iconSize / 2 - 64 / 2, 64, 64), Color.White);

                // Draw icon box
                if (this.IconSize == DetailsIconSize.Large)
                    spriteBatch.DrawOnCtrl(this, Content.GetTexture("605003"), new Rectangle(0, 0, iconSize, iconSize), Color.White);
            }

            // Draw bottom section seperator
            spriteBatch.DrawOnCtrl(this, Content.GetTexture("157218"), new Rectangle(this.ContentRegion.X, bounds.Height - 40, bounds.Width, 8), Color.White);

            // Wrap text
            string wrappedText = Utils.DrawUtil.WrapText(this.Font, this.Text, EVENTSUMMARY_WIDTH - 40 - iconSize - 20);

            // Draw name
            spriteBatch.DrawStringOnCtrl(this, wrappedText, this.Font, new Rectangle(iconSize + 20, 0, EVENTSUMMARY_WIDTH - 40, this.Height - BOTTOMSECTION_HEIGHT), Color.White, false, true, 2);
        }
    }
}
