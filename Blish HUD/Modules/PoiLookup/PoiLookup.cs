using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Modules.PoiLookup {
    public class PoiLookup : Module {

        public Dictionary<String, BHGw2Api.Landmark> PointsOfInterest = new Dictionary<string, BHGw2Api.Landmark>();

        private PoiLookupWindow LandmarkSearchWindow;
        private SkillBox LandmarkSearch;
        private CornerIcon landmarkSearchIcon;

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                "Landmark Lookup",
                null,
                "bh.general.landmarklookup",
                "Allows you to search for in game landmarks (waypoints, POIs, vistas, etc.) and copy the chat codes into your clipboard.",
                "LandersXanders.1235",
                "1"
            );
        }

        internal SettingEntry<bool> settingShowNotificationWhenLandmarkIsCopied;
        internal SettingEntry<bool> settingHideWindowAfterSelection;
        internal SettingEntry<bool> settingEnterSelectionIntoChatAutomatically;

        public override void DefineSettings(Settings settings) {
            settingShowNotificationWhenLandmarkIsCopied = settings.DefineSetting("Show Notification When Landmark Is Copied", true, true, true, "If enabled, a notification will be displayed in the center of the screen confirming the landmark was copied.");
            settingHideWindowAfterSelection = settings.DefineSetting("Hide Window After Selection", true, true, true, "If enabled, the landmark search window will automatically hide after a landmark is selected from the results.");
        }

        public override void OnEnabled() {
            LandmarkSearchWindow = new PoiLookupWindow(this) {
                Parent   = GameService.Graphics.SpriteScreen,
                Location = GameService.Graphics.SpriteScreen.Size / new Point(2)
            };

            LandmarkSearch = new SkillBox() {
                Location = new Point(GameService.Graphics.WindowWidth / 4 * 1 - 30, 0),
                Icon     = GameService.Content.GetTexture("landmark-search-icon"),
                Parent   = GameService.Graphics.SpriteScreen,
                Menu     = new ContextMenuStrip(),
                Visible  = false
            };

            LandmarkSearch.Menu.AddMenuItem("Use small icon").Click += delegate {
                LandmarkSearch.Visible     = false;
                landmarkSearchIcon.Visible = true;
            };

            landmarkSearchIcon = new CornerIcon() {
                Icon = GameService.Content.GetTexture("landmark-search"),
                HoverIcon = GameService.Content.GetTexture("landmark-search-hover"),
                Menu             = new ContextMenuStrip(),
                BasicTooltipText = "Landmark Search",
                Priority         = 5,
            };

            landmarkSearchIcon.Menu.AddMenuItem("Use large icon").Click += delegate {
                LandmarkSearch.Visible = true;
                landmarkSearchIcon.Visible = false;
            };

            landmarkSearchIcon.Click += delegate {
                LandmarkSearchWindow.ToggleWindow();
            };

            GameService.Graphics.SpriteScreen.Resized += delegate { LandmarkSearch.Location = new Point(GameService.Graphics.WindowWidth / 4 * 1 - 30, 0); };

            LandmarkSearch.LeftMouseButtonReleased += delegate {
                LandmarkSearchWindow.ToggleWindow();
            };

            var floorInfo = BHGw2Api.Floor.FloorFromContinentAndId(1, 1);

            if (floorInfo == null) {
                GameService.Debug.WriteWarningLine("PoiLookup: Could not load landmark information from API. Aborting.");
                return;
            }

            foreach (var region in floorInfo.Regions) {
                foreach (var map in region.Maps) {
                    foreach (var upoi in map.Landmarks) {
                        if (upoi.Name?.Length > 0 && !PointsOfInterest.ContainsKey(upoi.Name)) {
                            PointsOfInterest.Add(upoi.Name, upoi);
                        }
                    }
                }
            }
        }

        public override void OnDisabled() {
            base.OnDisabled();

            LandmarkSearch?.Dispose();
            LandmarkSearchWindow?.Dispose();
        }

        // TODO: Consider utilizing Parallel.For to speed this up a tad
        public BHGw2Api.Landmark GetClosestWaypoint(BHGw2Api.Landmark landmark) {
            BHGw2Api.Landmark closestWp = null;
            float distance = float.MaxValue;

            var staticPos = new Vector2((float)landmark.Coordinates.X, (float)landmark.Coordinates.Y);

            foreach (var wp in PointsOfInterest.Values.Where(poi => poi.Type == "waypoint" && landmark != poi)) {
                var pos = new Vector2((float)wp.Coordinates.X, (float)wp.Coordinates.Y);

                var netDist = Vector2.Distance(staticPos, pos);

                if (netDist < distance) {
                    closestWp = wp;
                    distance = netDist;
                }
            }
            
            return closestWp;
        }

    }
}
