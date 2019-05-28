using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api;
using Blish_HUD.Controls;
using Blish_HUD.Utils;
using Flurl;
using Humanizer;
using Microsoft.Scripting.Utils;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.EventTimers {
    public class EventTimers:Module {

        private const string DD_ALPHABETICAL = "Alphabetical";
        private const string DD_NEXTUP = "Next Up";

        private const string EC_ALLEVENTS = "All Events";
        private const string EC_HIDDEN = "Hidden Events";

        private const int NEXTTIME_WIDTH = 75;

        private List<DetailsButton> displayedEvents;
        
        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "Event Timers",
                null,
                "bh.general.events",
                "Displays upcoming events and gives you the option to subscribe to in-game notifications for when they're going to be starting soon.",
                "LandersXanders.1235",
                "1"
            );
        }

        private Settings allSettings;

        public override void DefineSettings(Settings settings) { allSettings = settings; }

        public override void OnEnabled() {
            base.OnEnabled();
            
            displayedEvents = new List<DetailsButton>();

            //AddSectionTab("World Boss and Meta Timers", "world-bosses", BuildSettingPanel());
            AddSectionTab("Events and Timers", GameService.Content.GetTexture("1466345"), BuildSettingPanel(GameService.Director.BlishHudWindow.ContentRegion));
        }

        private void RepositionES() {
            int pos = 0;
            foreach (var es in displayedEvents) {
                int x = pos % 2;
                int y = pos / 2;

                es.Location = new Point(x * 308, y * 108);

                if (es.Visible) pos++;

                // TODO: Just expose the panel to the module so that we don't have to do it this dumb way:
                ((Panel) es.Parent).VerticalScrollOffset = 0;
                es.Parent.Invalidate();
            }
        }

        private string GetTimeDetails(Meta AssignedMeta) {
            var timeUntil = AssignedMeta.NextTime - DateTime.Now;

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
            foreach (var utime in AssignedMeta.Times.Select(time => time > DateTime.UtcNow ? time.ToLocalTime() : time.ToLocalTime() + 1.Days()).OrderBy(time => time.Ticks).ToList()) {
                msg.Append(Environment.NewLine + utime.ToShortTimeString());
            }

            return msg.ToString();
        }

        private Panel BuildSettingPanel(Rectangle panelBounds) {
            var etPanel = new Panel() {
                CanScroll = false,
                Size      = panelBounds.Size
            };

            var eventPanel = new FlowPanel() {
                FlowDirection  = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(8, 8),
                Location  = new Point(etPanel.Width - 720 - 10 - 20, 50),
                Size      = new Point(748, etPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN),
                Parent    = etPanel,
                CanScroll = true,
            };

            var ddSortMethod = new Dropdown() {
                Parent   = etPanel,
                Location = new Point(etPanel.Right - 150 - 10, 5),
                Width    = 150
            };

            foreach (var meta in Meta.Events) {
                var setting = allSettings.DefineSetting("watchEvent:" + meta.Name, true, true, false, "");

                meta.IsWatched = setting.Value;

                var es2 = new DetailsButton {
                    Parent = eventPanel,
                    BasicTooltipText = meta.Category,
                    Text = meta.Name,
                    IconSize = DetailsIconSize.Small,
                    Icon = string.IsNullOrEmpty(meta.Icon) ? null : GameService.Content.GetTexture(meta.Icon),
                    ShowVignette = false,
                    HighlightType = DetailsHighlightType.LightHighlight,
                    ShowToggleButton = true
                };

                var nextTimeLabel = new Label() {
                    Size = new Point(65, es2.ContentRegion.Height),
                    Text = meta.NextTime.ToShortTimeString(),
                    BasicTooltipText = GetTimeDetails(meta),
                    HorizontalAlignment = DrawUtil.HorizontalAlignment.Center,
                    VerticalAlignment = DrawUtil.VerticalAlignment.Middle,
                    Parent = es2,
                };

                Adhesive.Binding.CreateOneWayBinding(() => nextTimeLabel.Height, () => es2.ContentRegion, (rectangle => rectangle.Height), true);

                if (!string.IsNullOrEmpty(meta.Wiki)) {
                    var glowWikiBttn = new GlowButton {
                        Icon             = GameService.Content.GetTexture("102530"),
                        ActiveIcon       = GameService.Content.GetTexture("glow-wiki"),
                        BasicTooltipText = "Read about this event on the wiki.",
                        Parent           = es2,
                        GlowColor        = Color.White * 0.1f
                    };

                    glowWikiBttn.Click += delegate {
                        if (Url.IsValid(meta.Wiki)) {
                            Process.Start(meta.Wiki);
                        }
                    };
                }

                if (!string.IsNullOrEmpty(meta.Waypoint)) {
                    var glowWaypointBttn = new GlowButton {
                        Icon             = GameService.Content.GetTexture("waypoint"),
                        ActiveIcon       = GameService.Content.GetTexture("glow-waypoint"),
                        BasicTooltipText = $"Nearby waypoint: {meta.Waypoint}",
                        Parent           = es2,
                        GlowColor        = Color.White * 0.1f
                    };

                    glowWaypointBttn.Click += delegate {
                        System.Windows.Forms.Clipboard.SetText(meta.Waypoint);

                        Controls.Notification.ShowNotification(GameService.Content.GetTexture("waypoint"), "Waypoint copied to clipboard.", 2);
                    };
                }

                var toggleFollowBttn = new GlowButton() {
                    Icon = GameService.Content.GetTexture("605021"),
                    ActiveIcon = GameService.Content.GetTexture("605019"),
                    BasicTooltipText = "Click to toggle tracking for this event.",
                    ToggleGlow = true,
                    Checked = meta.IsWatched,
                    Parent = es2,
                };

                toggleFollowBttn.Click += delegate {
                    meta.IsWatched = toggleFollowBttn.Checked;
                    setting.Value  = toggleFollowBttn.Checked;
                };

                meta.OnNextRunTimeChanged += delegate {
                    UpdateSort(ddSortMethod, EventArgs.Empty);

                    nextTimeLabel.Text = meta.NextTime.ToShortTimeString();
                    nextTimeLabel.BasicTooltipText = GetTimeDetails(meta);
                };

                displayedEvents.Add(es2);
            }

            var menuSection = new Panel {
                ShowBorder = true,
                Size       = new Point(etPanel.Width - 720 - 10 - 10 - 5 - 20, eventPanel.Height + Panel.BOTTOM_MARGIN),
                Location   = new Point(5,                                    50),
                Parent     = etPanel,
                Title      = "Event Categories"
            };

            // Add menu items for each category (and built-in categories)
            var eventCategories = new Menu {
                Size           = menuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent         = menuSection,
            };

            List<IGrouping<string, Meta>> submetas = Meta.Events.GroupBy(e => e.Category).ToList();

            var evAll = eventCategories.AddMenuItem(EC_ALLEVENTS);
            evAll.LeftMouseButtonReleased += delegate {
                displayedEvents.ForEach(de => { de.Visible = true; });

                //RepositionES();
            };

            foreach (IGrouping<string, Meta> e in submetas) {
                var ev = eventCategories.AddMenuItem(e.Key);
                ev.LeftMouseButtonReleased += delegate {
                    //displayedEvents.ForEach(de => { de.Visible = de.AssignedMeta.Category == e.Key; });
                    
                    //RepositionES();
                };
            }

            // TODO: Hidden events/timers to be added later
            //eventCategories.AddMenuItem(EC_HIDDEN);

            // Add dropdown for sorting events
            ddSortMethod.Items.Add(DD_ALPHABETICAL);
            ddSortMethod.Items.Add(DD_NEXTUP);

            ddSortMethod.ValueChanged += UpdateSort;

            ddSortMethod.SelectedItem = DD_NEXTUP;
            UpdateSort(ddSortMethod, EventArgs.Empty);

            Console.WriteLine("Main Panel is: " + etPanel.Location.ToString() + " :: " + etPanel.Size.ToString());
            Console.WriteLine("Event Panel is: " + eventPanel.Location.ToString() + " :: " + eventPanel.Size.ToString());
            Console.WriteLine("Menu Section Panel is: " + menuSection.Location.ToString() + " :: " + eventPanel.Size.ToString());

            return etPanel;
        }

        private void UpdateSort(object sender, EventArgs e) {
            switch (((Dropdown)sender).SelectedItem) {
                case DD_ALPHABETICAL:
                    //displayedEvents.Sort((e1, e2) => e1.AssignedMeta.Name.CompareTo(e2.AssignedMeta.Name));
                    break;
                case DD_NEXTUP:
                    //displayedEvents.Sort((e1, e2) => e1.AssignedMeta.NextTime.CompareTo(e2.AssignedMeta.NextTime));
                    break;
            }

            RepositionES();
        }

        public override void Update(GameTime gameTime) {
            // TODO: All event/meta stuff should be brought into this module (it's not part of the API)
            BHGw2Api.Meta.UpdateEventSchedules();
        }

        public override void OnDisabled() {
            displayedEvents.ForEach(de => de.Dispose());
            displayedEvents.Clear();

            base.OnDisabled();
        }

    }
}
