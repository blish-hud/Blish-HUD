using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Pathing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Modules.MarkersAndPaths {
    public class MarkersAndPaths : Module {

        internal const string MARKER_DIRECTORY = "markers";
        internal const string PATHS_DIRECTORY = "paths";

        private string MarkerDirectory => Path.Combine(GameService.FileSrv.BasePath, MARKER_DIRECTORY);
        private string PathsDirectory => Path.Combine(GameService.FileSrv.BasePath, PATHS_DIRECTORY);

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                  "Markers & Paths",
                  GameService.Content.GetTexture("157355"),
                  "bh.general.markersandpaths",
                  "Allows you to import markers and paths built for TacO and AugTyr.",
                  "LandersXanders.1235 (with additional code provided by BoyC)",
                  "1"
            );
        }

        public override void DefineSettings(Settings settings) {

        }

        public override void OnEnabled() {
            // Ensure both the marker and paths directory are available in the documents folder
            if (!Directory.Exists(this.MarkerDirectory)) Directory.CreateDirectory(this.MarkerDirectory);
            if (!Directory.Exists(this.PathsDirectory)) Directory.CreateDirectory(this.PathsDirectory);

            //AddSectionTab("Markers and Paths", GameService.Content.GetTexture("marker-pathing-icon"), GetPanel());

            // Could take a while to load in everything - offload it so that Blish HUD can finish starting
            // Load the markers and paths
            var prog = new Progress<string>((report) => GameService.Pathing.Icon.LoadingMessage = report);

            GameService.Debug.StartTimeFunc("Markers and Paths");
            var loadPacks = new Task(() => LoadPacks(prog));
            loadPacks.ContinueWith((result) => {
                GameService.Pathing.Icon.LoadingMessage = null;
                GameService.Debug.StopTimeFuncAndOutput("Markers and Paths");
            });
            loadPacks.Start();
        }

        private void LoadPacks(IProgress<string> progressIndicator) {
            string[] packFiles = Directory.GetFiles(this.MarkerDirectory);

            var standardDirPackContext = DirectoryPackContext.GetCachedContext(this.MarkerDirectory);
            GameService.Pathing.RegisterPathContext(standardDirPackContext);
            standardDirPackContext.LoadOnFileType(PackFormat.OverlayDataReader.ReadFromXmlPack, "xml", progressIndicator);

            foreach (string packFile in packFiles) {
                if (packFile.EndsWith(".zip")) {
                    // Potentially contains many packs within
                    var zipPackContext = ZipPackContext.GetCachedContext(packFile);
                    GameService.Pathing.RegisterPathContext(zipPackContext);
                    zipPackContext.LoadOnFileType(PackFormat.OverlayDataReader.ReadFromXmlPack, "xml", progressIndicator);
                }
            }

            GameService.Pathing.Icon.LoadingMessage = "Building category menues...";
            BuildCategoryMenus();
        }

        private void AddCategoryToMenuStrip(ContextMenuStrip parentMenuStrip, PackFormat.TacO.PathingCategory newCategory) {
            var newCategoryMenuItem = parentMenuStrip.AddMenuItem(newCategory.DisplayName);
            newCategoryMenuItem.CanCheck = true;
            newCategoryMenuItem.Checked  = newCategory.Visible;

            newCategoryMenuItem.CheckedChanged += delegate(object sender, CheckChangedEvent e) { newCategory.Visible = e.Checked; };

            if (newCategory.Any()) {
                var childMenuStrip = new ContextMenuStrip();
                newCategoryMenuItem.Submenu = childMenuStrip;

                foreach (var childCategory in newCategory) {
                    AddCategoryToMenuStrip(childMenuStrip, childCategory);
                }
            }
        }

        private void BuildCategoryMenus() {
            GameService.Director.QueueMainThreadUpdate((gameTime) => {
                                                      var rootCategoryMenu = new ContextMenuStrip();

                                                      var allMarkersCMS = new ContextMenuStripItem() {
                                                          Text     = "All markers",
                                                          Submenu  = rootCategoryMenu,
                                                          CanCheck = false
                                                      };

                                                      foreach (var childCategory in PackFormat.OverlayDataReader.Categories) {
                                                          AddCategoryToMenuStrip(rootCategoryMenu, childCategory);
                                                      }

                                                      allMarkersCMS.Parent = GameService.Pathing.IconContextMenu;
                                                  });
        }

    }
}
