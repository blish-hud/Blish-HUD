using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api;
using Blish_HUD.Controls;

// This is old
namespace Blish_HUD.Modules.EventTimers {
    public class EventView : Panel {

        private const int PANEL_PADDING = 10;
        private const int TIME_COLUMN_COUNT = 4;

        public Meta Meta { get; protected set; }

        public EventView(Meta meta) {
            this.Meta = meta;

            var lblEventTitle = new Label() {
                Text = meta.Name,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(PANEL_PADDING, PANEL_PADDING),
                Parent = this
            };

            int twidth = 0;

            int timeIndex = 0;
            foreach (var eventTime in meta.Times.OrderBy(e => e.ToLocalTime().TimeOfDay)) {
                int xPos = timeIndex % TIME_COLUMN_COUNT;
                int yPos = timeIndex / TIME_COLUMN_COUNT;

                var cbTimebox = new Checkbox() {
                    Text = eventTime.ToLocalTime().ToShortTimeString(),
                    Location = new Point(xPos * 100 + 100, yPos * 25 + lblEventTitle.Bottom + 5),
                    Parent = this,
                    TextColor = (DateTime.Now.TimeOfDay.CompareTo(eventTime.ToLocalTime().TimeOfDay) < 0) ? Color.White : Color.DarkGray
                };

                twidth += cbTimebox.Width;

                timeIndex++;
            }

            UpdateSize();
        }

        private void UpdateSize() {
            if (this.Children.Count > 0) {
                this.Width = this.Children.Max(c => c.Right) + PANEL_PADDING;
                this.Height = this.Children.Max(c => c.Bottom) + PANEL_PADDING;
            }
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintContainer(spriteBatch, bounds);

            //if (this.Parent != null && this.Parent.Children.IndexOf(this) % 2 == 0) {
            //    spriteBatch.Draw(ContentService.Textures.Pixel, bounds, Color.Black * 0.5f);
            //}
        }

    }
}
