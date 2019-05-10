using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
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
                  null,
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
            
            GameService.Debug.StartTimeFunc("LoadPacks");
            LoadPacks();
            GameService.Debug.StopTimeFuncAndOutput("LoadPacks");

            //AddSectionTab("Markers and Paths", GameService.Content.GetTexture("marker-pathing-icon"), GetPanel());

            // Could take a while to load in everything - offload it so that Blish HUD can finish starting
            // Load the markers and paths
            //var loadPacks = new Task(LoadPacks);
            //loadPacks.Start();

        }

        private Panel GetPanel() {
            const string PC_THISMAP = "This Map";
            const string PC_ADVENTURES = "Adventures";
            const string PC_ACHIEVEMENTS = "Achievements";
            const string PC_FESTIVALS = "Festivals";

            //var tInteract = new InteractionIndicator();
            //tInteract.Text = "Leatherworking Station";
            //tInteract.Show();

            var mpPanel = new Panel() {
                Size      = GameService.Director.BlishHudWindow.ContentRegion.Size,
                CanScroll = false
            };

            var mpItemsPanel = new FlowPanel() {
                FlowDirection  = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(8f, 8f),
                Location       = new Point(mpPanel.Width - 630, 50),
                Size           = new Point(630,                 mpPanel.Height - 50 - Panel.BOTTOM_MARGIN),
                Parent         = mpPanel,
                CanScroll      = true
            };

            var categoryMenuPanel = new Panel() {
                ShowBorder = true,
                Size       = new Point(mpPanel.Width - mpItemsPanel.Width - 15, mpItemsPanel.Height + Panel.BOTTOM_MARGIN),
                Location   = new Point(5,                                       50),
                CanScroll  = true,
                Title      = "Marker and Path Categories",
                Parent     = mpPanel
            };

            var mpCategories = new Menu() {
                Size           = categoryMenuPanel.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent         = categoryMenuPanel
            };

            var recats = ReCatReader.FromFile(@"data\recategory.inf");

            var foldedCats = recats.GroupBy(section => section.SectionName.Split('.')[0])
                                   .Select(group => new {Category = group.Key, Sections = group.ToList()})
                                   .ToList();

            var thisMapMenuItem = new MenuItem() {
                Text     = PC_THISMAP,
                Icon     = GameService.Content.GetTexture("1431767"),
                CanCheck = true,
                Parent   = mpCategories
            };

            var allCats = new List<PathingCategory>();

            var dispPacks = new List<DetailsButton>();

            foreach (var tSec in foldedCats) {
                var nMenuItem = new MenuItem() {
                    Text     = tSec.Category,
                    CanCheck = true,
                    Parent   = mpCategories
                };

                foreach (var sSec in tSec.Sections) {
                    var refCats = new List<PathingCategory>();

                    foreach (string sVal in sSec.Values) {
                        refCats.Add(GameService.Pathing.Categories.GetOrAddCategoryFromNamespace(sVal));
                    }

                    if (refCats.Any()) {
                        var sMenuItem = new MenuItem() {
                            Text     = sSec.SectionName.Split('.')[1],
                            CanCheck = true,
                            Parent   = nMenuItem
                        };

                        sMenuItem.Click += delegate {
                            dispPacks.ForEach(pack => pack.Visible = sSec.Values.Contains(pack.BasicTooltipText));
                            //refCats.ForEach(c => c.Visible = !c.Visible);
                        };

                        allCats.AddRange(refCats);
                    }
                }
            }

            foreach (var cat in allCats) {
                //var ficon = cat.Pathables.FirstOrDefault()?.Texture;
                //ficon = ficon ?? cat.Paths.FirstOrDefault()?.PathTexture;

                //var ficon = cat.Pathables.FirstOrDefault()?.Icon;

                var pack = new DetailsButton() {
                    Parent           = mpItemsPanel,
                    BasicTooltipText = cat.Namespace,
                    Text             = cat.DisplayName,
                    //Icon             = ficon
                };

                dispPacks.Add(pack);
            }

            void AddCategoryToMenu(PathingCategory category, MenuItem parentMenuItem) {
                var newCat = new MenuItem(category.DisplayName) {
                    CanCheck = true,
                    Parent   = parentMenuItem
                };

                foreach (var subCategory in category) {
                    AddCategoryToMenu(subCategory, newCat);
                }
            }

            foreach (var cat in GameService.Pathing.Categories) {
                var newCat = new MenuItem(cat.DisplayName) {
                    CanCheck = true,
                    Parent = mpCategories
                };

                foreach (var subCategory in cat) {
                    AddCategoryToMenu(subCategory, newCat);
                }
            }

            return mpPanel;
        }

        private void LoadPacks() {
            string[] packFiles = Directory.GetFiles(this.MarkerDirectory);

            var standardDirPackContext = new DirectoryPackContext(this.MarkerDirectory);
            GameService.Pathing.RegisterPathContext(standardDirPackContext);
            standardDirPackContext.LoadOnXmlPack(PackFormat.OverlayDataReader.ReadFromXmlPack);

            foreach (string packfile in packFiles) {
                if (packfile.EndsWith(".xml")) {
                    // Load single pack
                    // NOOP
                } else if (packfile.EndsWith(".zip")) {
                    // Potentially contains many packs within
                    var zipPackContext = new ZipPackContext(packfile);
                    GameService.Pathing.RegisterPathContext(zipPackContext);
                    zipPackContext.LoadOnXmlPack(PackFormat.OverlayDataReader.ReadFromXmlPack);
                }
            }

            //PrintOutCategories(GameService.Pathing.Categories);
        }

        private void PrintOutCategories(PathingCategory curCategory, int depth = 0) {
            Console.WriteLine($"{new String('\t', depth)}{curCategory.Name}");
            //Console.WriteLine(curCategory.Namespace);

            foreach (var childCategory in curCategory) {
                PrintOutCategories(childCategory, depth + 1);
            }
        }

    }
}
