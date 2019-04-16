using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public enum DetailsIconSize {
        Small,
        Large
    }

    /// <summary>
    /// Used to show details and multiple options for a single topic. Designed to look like the Achievements details icon.
    /// </summary>
    public class DetailsButton : ScrollingButtonContainer {

        private const int EVENTSUMMARY_WIDTH  = 300; //354;
        private const int EVENTSUMMARY_HEIGHT = 100;
        private const int BOTTOMSECTION_HEIGHT = 35;

        private DetailsIconSize _iconSize = DetailsIconSize.Large;
        public DetailsIconSize IconSize {
            get => _iconSize;
            set {
                if (_iconSize == value) return;

                _iconSize = value;
                OnPropertyChanged();

                UpdateContentRegion();
            }
        }

        private string _text;
        public string Text {
            get => _text;
            set {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged();
            }
        }

        private Texture2D _icon;
        public Texture2D Icon {
            get => _icon;
            set {
                if (_icon == value) return;

                _icon = value;
                OnPropertyChanged();
            }
        }

        public DetailsButton() {
            this.Size = new Point(EVENTSUMMARY_WIDTH, EVENTSUMMARY_HEIGHT);

            UpdateContentRegion();
        }

        private void UpdateContentRegion() {
            int bottomRegionLeft = EVENTSUMMARY_HEIGHT;

            if (this.IconSize == DetailsIconSize.Small) bottomRegionLeft = 0;

            this.ContentRegion = new Rectangle(bottomRegionLeft, this.Height - BOTTOMSECTION_HEIGHT, this.Width - bottomRegionLeft, BOTTOMSECTION_HEIGHT);
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw background
            spriteBatch.Draw(ContentService.Textures.Pixel, bounds, Color.Black * 0.25f);

            // Draw bottom section
            spriteBatch.Draw(ContentService.Textures.Pixel, this.ContentRegion, Color.Black * 0.1f);
            
            int iconSize = this.IconSize == DetailsIconSize.Large ? EVENTSUMMARY_HEIGHT : EVENTSUMMARY_HEIGHT - BOTTOMSECTION_HEIGHT;

            if (this.Icon != null) {
                // Draw icon (the ternary in there is nasty - need to do a better way to offset this
                spriteBatch.Draw(this.Icon, new Rectangle(iconSize / 2 - 64 / 2 + (this.IconSize == DetailsIconSize.Small ? 10 : 0), iconSize / 2 - 64 / 2, 64, 64), Color.White);

                // Draw icon box
                if (this.IconSize == DetailsIconSize.Large)
                    spriteBatch.Draw(Content.GetTexture("605003"), new Rectangle(0, 0, iconSize, iconSize), Color.White);
            }

            // Draw bottom section seperator
            spriteBatch.Draw(Content.GetTexture("157218"), new Rectangle(this.ContentRegion.X, bounds.Height - 40, bounds.Width, 8), Color.White);

            // Wrap text
            string wrappedText = Utils.DrawUtil.WrapText(Content.DefaultFont14, this.Text, EVENTSUMMARY_WIDTH - 40 - iconSize - 20);

            // Draw name of event multiple times for cheap stroke effect
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(iconSize + 20 - 2, 0 - 2, EVENTSUMMARY_WIDTH - 40, this.Height - BOTTOMSECTION_HEIGHT), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(iconSize + 20 + 2, 0 + 2, EVENTSUMMARY_WIDTH - 40, this.Height - BOTTOMSECTION_HEIGHT), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(iconSize + 20 - 2, 0 + 2, EVENTSUMMARY_WIDTH - 40, this.Height - BOTTOMSECTION_HEIGHT), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(iconSize + 20 + 2, 0 - 2, EVENTSUMMARY_WIDTH - 40, this.Height - BOTTOMSECTION_HEIGHT), Color.Black);

            // Draw name of event
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(iconSize + 20,     0,     EVENTSUMMARY_WIDTH - 40, this.Height - BOTTOMSECTION_HEIGHT), Color.White);
        }

    }
}
