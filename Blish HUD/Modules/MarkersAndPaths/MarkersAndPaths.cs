using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.MarkersAndPaths {
    public class MarkersAndPaths : Module {

        internal const string MARKER_DIRECTORY = "markers";
        internal const string PATHS_DIRECTORY = "paths";

        private string MarkerDirectory => Path.Combine(GameService.FileSrv.BasePath, MARKER_DIRECTORY);
        private string PathsDirectory => Path.Combine(GameService.FileSrv.BasePath, PATHS_DIRECTORY);

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
                                  "(General) Markers & Paths",
                                  "bh.general.markersandpaths",
                                  "Allows you to import markers and paths built for TacO and AugTyr.",
                                  "LandersXanders.1235 (with additional code provided by BoyC)",
                                  "1"
                                 );
        }

        public override void DefineSettings(Settings settings) {

        }

        protected override void OnEnabled() {
            // Ensure both the marker and paths directory are available in the documents folder
            if (!Directory.Exists(this.MarkerDirectory)) Directory.CreateDirectory(this.MarkerDirectory);
            if (!Directory.Exists(this.PathsDirectory)) Directory.CreateDirectory(this.MarkerDirectory);

            // Could take a while to load in everything - offload it so that Blish HUD can finish starting
            // Load the markers and paths
            //var loadPacks = new Task(LoadPacks);
            //loadPacks.Start();
            LoadPacks();
        }
        
        private void LoadPacks() {
            string[] packFiles = Directory.GetFiles(this.MarkerDirectory);

            foreach (string packfile in packFiles) {
                if (packfile.EndsWith(".xml")) { // Load single pack
                    Console.WriteLine($"Loading pack file {packfile}");
                    PackFormat.OverlayDataReader.ReadFromXmlFile(packfile);
                } else if (packfile.EndsWith(".zip")) { // Contains many packs within
                    Console.WriteLine($"Found pack file {packfile}, but can't open it because ZIP hasn't been implemented yet!");
                }
            }

            PrintOutCategories(GameService.Pathing.Categories);
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
