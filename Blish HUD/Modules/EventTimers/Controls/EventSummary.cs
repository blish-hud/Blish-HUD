using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api;
using Blish_HUD.Controls;

namespace Blish_HUD.Modules.EventTimers {
    public class EventSummary:Control {

        private const int EVENTSUMMARY_WIDTH = 300;
        private const int EVENTSUMMARY_HEIGHT = 100;

        private const int NEXTTIME_WIDTH = 75;
        private const int BOTTOMSECTION_HEIGHT = 35;

        private static Texture2D BackgroundSprite;
        private static Texture2D EyeSprite;
        private static Texture2D EyeSelectedSprite;
        private static Texture2D HoverEyeSprite;
        private static Texture2D SelectedSprite;
        private static Texture2D DividerSprite;
        private static Texture2D EyeBackgroundSprite;
        private static Texture2D WaypointSprite;
        private static Texture2D WikiSprite;
        private static Texture2D IconBoxSprite;

        private bool _mouseOverEye = false;
        private bool MouseOverEye {
            get => _mouseOverEye;
            set => SetProperty(ref _mouseOverEye, value);
        }

        private bool _mouseOverTime = false;
        private bool MouseOverTime {
            get => _mouseOverTime;
            set => SetProperty(ref _mouseOverTime, value);
        }

        private bool _mouseOverWaypoint = false;
        private bool MouseOverWaypoint {
            get => _mouseOverWaypoint;
            set => SetProperty(ref _mouseOverWaypoint, value);
        }

        private bool _mouseOverWiki = false;
        private bool MouseOverWiki {
            get => _mouseOverWiki;
            set => SetProperty(ref _mouseOverWiki, value);
        }

        public bool Active {
            get => this.AssignedMeta.IsWatched;
            set {
                watchState.Value = value;
                this.AssignedMeta.IsWatched = value;
                Invalidate();
            }
        }

        private SettingEntry<bool> watchState;

        public Meta AssignedMeta { get; protected set; }

        public EventSummary(Meta meta, SettingsManager settingsManager) {
            this.AssignedMeta = meta;

            watchState = settingsManager.DefineSetting("watchEvent:" + meta.Name, true, true, false, "");
            this.AssignedMeta.IsWatched = watchState.Value;

            BackgroundSprite = BackgroundSprite ?? ContentService.Textures.Pixel;
            EyeSprite = EyeSprite ?? Content.GetTexture("605021");
            EyeSelectedSprite = EyeSelectedSprite ?? Content.GetTexture("605019");
            HoverEyeSprite = HoverEyeSprite ?? Content.GetTexture("605000-2");
            DividerSprite = DividerSprite ?? Content.GetTexture("157218");
            EyeBackgroundSprite = EyeBackgroundSprite ?? Content.GetTexture("605011");
            IconBoxSprite = IconBoxSprite ?? Content.GetTexture("605003");
            WaypointSprite = WaypointSprite ?? Content.GetTexture("waypoint");
            WikiSprite = WikiSprite ?? Content.GetTexture("102530");

            this.Size = new Point(EVENTSUMMARY_WIDTH, EVENTSUMMARY_HEIGHT);

            this.MouseMoved += EventSummary_MouseMoved;
            this.MouseLeft += EventSummary_MouseLeft;
            this.LeftMouseButtonPressed += EventSummary_LeftMouseButtonPressed;
        }

        private void EventSummary_LeftMouseButtonPressed(object sender, MouseEventArgs e) {
            if (this.MouseOverEye) this.Active = !this.Active;
            if (this.MouseOverWiki) Process.Start(this.AssignedMeta.Wiki);

            if (this.MouseOverWaypoint) {
                try {
                    System.Windows.Forms.Clipboard.SetText(this.AssignedMeta.Waypoint);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }

                //if (Module.settingShowNotificationWhenLandmarkIsCopied.Value)
                Notification.ShowNotification("Waypoint copied to clipboard.");
            }
        }

        private void EventSummary_MouseLeft(object sender, MouseEventArgs e) {
            this.MouseOverEye = false;
            this.MouseOverTime = false;
            this.MouseOverWaypoint = false;
            this.MouseOverWiki = false;
        }

        private void EventSummary_MouseMoved(object sender, MouseEventArgs e) {
            var relPos = RelativeMousePosition;

            if (this.MouseOver && relPos.Y > this.Height - BOTTOMSECTION_HEIGHT) {
                this.MouseOverEye = relPos.X > this.Width - BOTTOMSECTION_HEIGHT;
                this.MouseOverTime = relPos.X <= NEXTTIME_WIDTH + 15;
                this.MouseOverWiki = !this.MouseOverTime && relPos.X < NEXTTIME_WIDTH + 15 + 32;
                this.MouseOverWaypoint = !this.MouseOverTime && !this.MouseOverWiki && relPos.X < NEXTTIME_WIDTH + 15 + 32 + 35 + 5;
            } else {
                this.MouseOverEye = false;
                this.MouseOverTime = false;
                this.MouseOverWiki = false;
                this.MouseOverWaypoint = false;
            }

            this.MouseOverWaypoint = this.MouseOverWaypoint && !string.IsNullOrEmpty(this.AssignedMeta.Waypoint);
            this.MouseOverWiki = this.MouseOverWiki && !string.IsNullOrEmpty(this.AssignedMeta.Wiki);

            if (this.MouseOverEye)
                this.BasicTooltipText = "Click to toggle tracking for this event.";
            else if (this.MouseOverWiki)
                this.BasicTooltipText = "Read about this event on the wiki.";
            else if (this.MouseOverWaypoint)
                this.BasicTooltipText = $"Nearby waypoint: {AssignedMeta.Waypoint}";
            else if (this.MouseOverTime)
                this.BasicTooltipText = GetTimeDetails();
            else
                this.BasicTooltipText = this.AssignedMeta.Category;
        }

        private string GetTimeDetails() {
            var timeUntil = this.AssignedMeta.NextTime - DateTime.Now;

            var msg = new StringBuilder();

            msg.AppendLine("Starts in " + 
                timeUntil.Humanize(
                    maxUnit: Humanizer.Localisation.TimeUnit.Hour,
                    minUnit: Humanizer.Localisation.TimeUnit.Minute,
                    precision: 2,
                    collectionSeparator: null
                )
            );

            msg.Append(Environment.NewLine + "Upcoming Event Times:");
            foreach (var utime in this.AssignedMeta.Times.Select(time => time > DateTime.UtcNow ? time.ToLocalTime() : time.ToLocalTime() + 1.Days()).OrderBy(time => time.Ticks).ToList()) {
                msg.Append(Environment.NewLine + utime.ToShortTimeString());
            }
            
            return msg.ToString();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw background
            spriteBatch.Draw(BackgroundSprite, _size.InBounds(bounds), Color.Black * 0.25f);

            // Draw eye highlight
            if (this.MouseOverEye) {
                spriteBatch.Draw(HoverEyeSprite,
                                 new Rectangle(_size.X - 256, 0, 256, 100).ToBounds(bounds),
                                 Color.White);
            } else {
                spriteBatch.Draw(EyeBackgroundSprite,
                                 new Rectangle(_size - new Point(35, 35), new Point(35, 35)).ToBounds(bounds),
                                 Color.Black);
            }

            // Draw bottom section (overlap to make background darker here)
            spriteBatch.Draw(BackgroundSprite,
                             new Rectangle(0, _size.Y - BOTTOMSECTION_HEIGHT, _size.X - BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT).ToBounds(bounds),
                             Color.Black * 0.1f);

            // Draw eye icon
            spriteBatch.Draw(this.Active 
                                 ? EyeSelectedSprite 
                                 : EyeSprite,
                             new Rectangle(_size - new Point(BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT),
                                           new Point(BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT)).ToBounds(bounds),
                             Color.White);

            // Draw wiki icon
            if (!string.IsNullOrEmpty(this.AssignedMeta.Wiki)) {
                if (this.MouseOverWiki)
                    spriteBatch.Draw(Content.GetTexture("glow-wiki"),
                                     new Rectangle(NEXTTIME_WIDTH + 15, _size.Y - BOTTOMSECTION_HEIGHT + 1, 32, 32).ToBounds(bounds),
                                     Color.White);

                spriteBatch.Draw(WikiSprite,
                                 new Rectangle(NEXTTIME_WIDTH + 15, _size.Y - BOTTOMSECTION_HEIGHT + 1, 32, 32).ToBounds(bounds),
                                 Color.White); 
            }

            // Draw waypoint icon
            if (!string.IsNullOrEmpty(this.AssignedMeta.Waypoint)) {
                if (this.MouseOverWaypoint)
                    spriteBatch.Draw(Content.GetTexture("glow-waypoint"),
                                     new Rectangle(NEXTTIME_WIDTH + 15 + 35, _size.Y - BOTTOMSECTION_HEIGHT + 1, 32, 32).ToBounds(bounds),
                                     Color.White);

                spriteBatch.Draw(WaypointSprite,
                                 new Rectangle(NEXTTIME_WIDTH + 15 + 35, _size.Y - BOTTOMSECTION_HEIGHT + 1, 32, 32).ToBounds(bounds),
                                 Color.White);
            }

            // Draw bottom section seperator
            spriteBatch.Draw(DividerSprite,
                             new Rectangle(0, _size.Y - 40, _size.X, 8).ToBounds(bounds),
                             Color.White);

            // Draw event icon
            if (this.AssignedMeta.Icon != null)
                spriteBatch.Draw(Content.GetTexture(AssignedMeta.Icon),
                                 new Rectangle((bounds.Height - BOTTOMSECTION_HEIGHT) / 2 - 32 + 10,
                                               (bounds.Height - 35) / 2 - 32,
                                               64,
                                               64).ToBounds(bounds),
                                 Color.White);

            //spriteBatch.Draw(IconBoxSprite, new Rectangle(0, 0, bounds.Height - 35, bounds.Height - 35), Color.White);

            string wrappedText = Utils.DrawUtil.WrapText(Content.DefaultFont14, this.AssignedMeta.Name, 200);
            // Draw name of event (multiple times for stroke effect)
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(89 - 2, 0 - 2, 216, _size.Y - BOTTOMSECTION_HEIGHT).ToBounds(bounds), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(89 + 2, 0 + 2, 216, _size.Y - BOTTOMSECTION_HEIGHT).ToBounds(bounds), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(89 - 2, 0 + 2, 216, _size.Y - BOTTOMSECTION_HEIGHT).ToBounds(bounds), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(89 + 2, 0 - 2, 216, _size.Y - BOTTOMSECTION_HEIGHT).ToBounds(bounds), Color.Black);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.DefaultFont14, wrappedText, new Rectangle(89, 0, 216, _size.Y - BOTTOMSECTION_HEIGHT).ToBounds(bounds), Color.White);
            
            // Draw the upcoming event time
            Utils.DrawUtil.DrawAlignedText(spriteBatch,
                                           Content.DefaultFont14,
                                           AssignedMeta.NextTime.ToShortTimeString(),
                                           new Rectangle(0, _size.Y - BOTTOMSECTION_HEIGHT, NEXTTIME_WIDTH, 35).ToBounds(bounds),
                                           Color.White,
                                           Utils.DrawUtil.HorizontalAlignment.Right);
        }

    }
}
