using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat {
    public static class OverlayDataReader {

        private const string ROOT_ELEMENT_MARKERCATEGORY = "markercategory";

        private static void TryLoadCategories(XmlDocument packDocument) {
            var categoryNodes = packDocument.DocumentElement?.SelectNodes("/OverlayData/MarkerCategory");
            if (categoryNodes == null) return;

            foreach (XmlNode categoryNode in categoryNodes) {
                PathingCategoryBuilder.UnpackCategory(categoryNode, GameService.Pathing.Categories);
            }
        }

        private static void TryLoadPOIs(XmlDocument packDocument) {
            var poiNodes = packDocument.DocumentElement?.SelectSingleNode("/OverlayData/POIs");
            if (poiNodes == null) return;

            foreach (XmlNode poiNode in poiNodes) {
                PoiBuilder.UnpackPoi(poiNode);
            }
        }

        public static void ReadFromXmlFile(string packPath) {
            if (!File.Exists(packPath)) {
                GameService.Debug.WriteWarning($"Could not find pack '{packPath}'.");
                return;
            }

            var packDocument = new XmlDocument();
            string packSrc = File.ReadAllText(packPath); //SanitizeXml(File.ReadAllText(packPath));

            //try {

                packDocument.LoadXml(packSrc);

                TryLoadCategories(packDocument);

                TryLoadPOIs(packDocument);

            //} catch (Exception exception) {
            //    GameService.Debug.WriteErrorLine("Could not load tacO overlay file {0}.  Error: {1}", packPath, exception.Message);
            //}
        }

        private static string SanitizeXml(string xmlDoc) {
            // TODO: Ask Tekkit to fix syntax
            return xmlDoc
                  .Replace("=\"<",      "=\"&lt;")
                  .Replace(">\">",      "&gt;\">")
                  .Replace("*",         "")
                  .Replace("0behavior", "behavior");
        }

    }
}
