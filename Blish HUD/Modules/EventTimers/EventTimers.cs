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
using Microsoft.Scripting.Utils;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.EventTimers {
    public class EventTimers:Module {

        private const string DD_ALPHABETICAL = "Alphabetical";
        private const string DD_NEXTUP = "Next Up";

        private const string EC_ALLEVENTS = "All Events";
        private const string EC_HIDDEN = "Hidden Events";

        private const int NEXTTIME_WIDTH = 75;

        private List<EventSummary> displayedEvents;

        protected override void OnLoad() {
            base.OnLoad();
        }

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "(General) Event Timers Module",
                "bh.general.events",
                "Displays upcoming events and gives you the option to subscribe to in-game notifications for when they're going to be starting soon.",
                "LandersXanders.1235",
                "1"
            );
        }

        private Settings allSettings;

        public override void DefineSettings(Settings settings) { allSettings = settings; }

        protected override void OnEnabled() {
            base.OnEnabled();
            
            displayedEvents = new List<EventSummary>();

            //AddSectionTab("World Boss and Meta Timers", "world-bosses", BuildSettingPanel());
            AddSectionTab("Events and Timers", "1466345", BuildSettingPanel());
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

        private Panel BuildSettingPanel() {
            var etPanel = new Panel() {
                CanScroll = false,
                Size      = GameService.Director.BlishHudWindow.ContentRegion.Size
            };

            var eventPanel = new Panel() {
                //FlowDirection  = FlowPanel.ControlFlowDirection.LeftToRight,
                //ControlPadding = 8,
                Location  = new Point(etPanel.Width - 630, 50),
                Size      = new Point(630,                  etPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN),
                Parent    = etPanel,
                CanScroll = true,
            };

            var ddSortMethod = new Dropdown() {
                Parent   = etPanel,
                Location = new Point(etPanel.Right - 150 - 10, 5),
                Width    = 150
            };

            foreach (var meta in Meta.Events) {
                var es = new EventSummary(meta, allSettings) {
                    Parent = eventPanel,
                    BasicTooltipText = meta.Category
                };

                // TODO: This will soon replace the current implementation (as soon as it has feature parity)

                //var es2 = new DetailsButton {
                //    Parent           = eventPanel,
                //    BasicTooltipText = meta.Category,
                //    Text             = meta.Name,
                //    IconSize         = DetailsIconSize.Small,
                //    Icon             = string.IsNullOrWhiteSpace(meta.Icon) ? null : GameService.Content.GetTexture(meta.Icon),
                //};

                //var nextTimeLabel = new Label {
                //    Size                = new Point(NEXTTIME_WIDTH, es2.ContentRegion.Height),
                //    Text                = meta.NextTime.ToShortTimeString(),
                //    HorizontalAlignment = DrawUtil.HorizontalAlignment.Right,
                //    VerticalAlignment   = DrawUtil.VerticalAlignment.Middle,
                //    Parent              = es2
                //};

                //if (!string.IsNullOrWhiteSpace(meta.Wiki)) {
                //    var glowWikiBttn = new GlowButton {
                //        Icon   = GameService.Content.GetTexture("102530"),
                //        Left   = NEXTTIME_WIDTH + 10,
                //        Parent = es2
                //    };

                //    glowWikiBttn.OnClick += delegate {
                //        if (Url.IsValid(meta.Wiki)) {
                //            Process.Start(meta.Wiki);
                //        }
                //    };
                //}

                //if (!string.IsNullOrWhiteSpace(meta.Waypoint)) {
                //    var glowWaypointBttn = new GlowButton {
                //        Icon   = GameService.Content.GetTexture("waypoint"),
                //        Left   = NEXTTIME_WIDTH + 32 + 10,
                //        Parent = es2
                //    };

                //    glowWaypointBttn.OnClick += delegate {
                //        System.Windows.Forms.Clipboard.SetText(meta.Waypoint);

                //        Controls.Notification.ShowNotification(
                //                                               GameService.Content.GetTexture("waypoint"), 
                //                                               "Waypoint copied to clipboard.", 
                //                                               2
                //                                               );
                //    };
                //}


                meta.OnNextRunTimeChanged += delegate {
                    UpdateSort(ddSortMethod, EventArgs.Empty);
                };

                displayedEvents.Add(es);
            }

            var menuSection = new Panel {
                ShowBorder = true,
                Size       = new Point(etPanel.Width - eventPanel.Width - 10, eventPanel.Height + Panel.BOTTOM_MARGIN),
                Location   = new Point(5,                               50),
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

                RepositionES();
            };

            foreach (IGrouping<string, Meta> e in submetas) {
                var ev = eventCategories.AddMenuItem(e.Key);
                ev.LeftMouseButtonReleased += delegate {
                    displayedEvents.ForEach(de => { de.Visible = de.AssignedMeta.Category == e.Key; });
                    
                    RepositionES();
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

            return etPanel;
        }

        private void UpdateSort(object sender, EventArgs e) {
            switch (((Dropdown)sender).SelectedItem) {
                case DD_ALPHABETICAL:
                    displayedEvents.Sort((e1, e2) => e1.AssignedMeta.Name.CompareTo(e2.AssignedMeta.Name));
                    break;
                case DD_NEXTUP:
                    displayedEvents.Sort((e1, e2) => e1.AssignedMeta.NextTime.CompareTo(e2.AssignedMeta.NextTime));
                    break;
            }

            RepositionES();
        }

        public override void Update(GameTime gameTime) {
            // TODO: All event/meta stuff should be brought into this module (it's not part of the API)
            BHGw2Api.Meta.UpdateEventSchedules();
        }

        protected override void OnDisabled() {
            displayedEvents.ForEach(de => de.Dispose());
            displayedEvents.Clear();

            base.OnDisabled();
        }

    }
}
