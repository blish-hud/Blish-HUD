using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Modules.MarkersAndPaths {
    public class MarkersAndPaths : Module {

        internal const string MARKER_DIRECTORY = "markers";
        internal const string PATHS_DIRECTORY = "paths";

        private string MarkerDirectory => Path.Combine(GameService.Directory.BasePath, MARKER_DIRECTORY);
        private string PathsDirectory => Path.Combine(GameService.Directory.BasePath, PATHS_DIRECTORY);

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                  "Markers & Paths 2",
                  GameService.Content.GetTexture("157355"),
                  "bh.general.markersandpaths2",
                  "Allows you to import markers and paths built for TacO and AugTyr.",
                  "LandersXanders.1235 (with additional code provided by BoyC)",
                  "1"
            );
        }

        public override void DefineSettings(SettingsManager settingsManager) {

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

            GameService.Pathing.NewMapLoaded += delegate { OverlayDataReader.UpdatePathableStates(); };
        }

        private void LoadPacks(IProgress<string> progressIndicator) {
            var dirDataReader = new Content.DirectoryReader(this.MarkerDirectory);
            var dirResourceManager = new PathableResourceManager(dirDataReader);
            GameService.Pathing.RegisterPathableResourceManager(dirResourceManager);
            dirDataReader.LoadOnFileType((Stream fileStream, IDataReader dataReader) => {
                PackFormat.OverlayDataReader.ReadFromXmlPack(fileStream, dirResourceManager);
            }, ".xml");

            // TODO: Cleanup
            string[] packFiles = Directory.GetFiles(this.MarkerDirectory, "*.zip", SearchOption.AllDirectories);
            foreach (string packFile in packFiles) {
                // Potentially contains many packs within
                var zipDataReader = new Content.ZipArchiveReader(packFile);
                var zipResourceManager = new PathableResourceManager(dirDataReader);
                GameService.Pathing.RegisterPathableResourceManager(zipResourceManager);
                zipDataReader.LoadOnFileType((Stream fileStream, IDataReader dataReader) => {
                    PackFormat.OverlayDataReader.ReadFromXmlPack(fileStream, zipResourceManager);
                }, ".xml");
            }

            GameService.Pathing.Icon.LoadingMessage = "Building category menus...";
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
