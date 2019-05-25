using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat {
    public static class OverlayDataReader {

        private const string ROOT_ELEMENT_MARKERCATEGORY = "markercategory";

        internal static readonly PathingCategory Categories = new PathingCategory("root") { Visible = true };

        public static void ReadFromXmlPack(string xmlPackContents, IPackFileSystemContext packContext) {
            var    packDocument = new XmlDocument();
            string packSrc      = SanitizeXml(xmlPackContents);
            bool   packLoaded   = false;

            try {
                packDocument.LoadXml(packSrc);
                packLoaded = true;
            } catch (XmlException exception) {
                GameService.Debug.WriteErrorLine($"Could not load tacO overlay file {packContext} from context {xmlPackContents.GetType().Name} due to an XML error.  Error: {exception.Message}");
            } catch (Exception exception) {
                throw;
            }

            if (packLoaded) {
                TryLoadCategories(packDocument);
                TryLoadPOIs(packDocument, packContext, Categories);
            }
        }

        private static void TryLoadCategories(XmlDocument packDocument) {
            var categoryNodes = packDocument.DocumentElement?.SelectNodes("/OverlayData/MarkerCategory");
            if (categoryNodes == null) return;

            foreach (XmlNode categoryNode in categoryNodes) {
                PathingCategoryBuilder.UnpackCategory(categoryNode, Categories);
            }
        }

        private static void TryLoadPOIs(XmlDocument packDocument, IPackFileSystemContext packContext, PathingCategory rootCategory) {
            var poiNodes = packDocument.DocumentElement?.SelectSingleNode("/OverlayData/POIs");
            if (poiNodes == null) return;

            foreach (XmlNode poiNode in poiNodes) {
                PoiBuilder.UnpackPathable(poiNode, packContext, rootCategory);
            }
        }

        private static string SanitizeXml(string xmlDoc) {
            // TODO: Ask Tekkit (and others) to fix syntax
            return xmlDoc
                  .Replace("&", "&amp;")
                  .Replace("=\"<",      "=\"&lt;")
                  .Replace(">\">",      "&gt;\">")
                  .Replace("*",         "")
                  .Replace("0behavior", "behavior");
        }

    }
}
