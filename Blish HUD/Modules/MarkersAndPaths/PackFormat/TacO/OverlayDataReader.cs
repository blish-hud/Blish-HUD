using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Content;
using Blish_HUD.Entities;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Pathables;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat {
    public static class OverlayDataReader {

        internal static readonly PathingCategory Categories = new PathingCategory("root") { Visible = true };

        internal static readonly List<IPathable<Entity>> Pathables = new List<IPathable<Entity>>();

        public static void RegisterPathable(IPathable<Entity> pathable) {
            if (pathable == null) return;

            Pathables.Add(pathable);
        }

        public static void UpdatePathableStates() {
            for (int i = 0; i < Pathables.Count - 1; i++)
                ProcessPathableState(Pathables[i]);
        }

        private static void ProcessPathableState(IPathable<Entity> pathable) {
            if (pathable.MapId == GameService.Player.MapId || pathable.MapId == -1) {
                pathable.Active = true;
                GameService.Pathing.RegisterPathable(pathable);
            } else if (GameService.Graphics.World.Entities.Contains(pathable.ManagedEntity)) {
                pathable.Active = false;
                GameService.Pathing.UnregisterPathable(pathable);
            }
        }

        public static void ReadFromXmlPack(Stream xmlPackStream, PathableResourceManager pathableResourceManager) {
            string xmlPackContents;

            using (var xmlReader = new StreamReader(xmlPackStream)) {
                xmlPackContents = xmlReader.ReadToEnd();
            }

            var    packDocument = new XmlDocument();
            string packSrc      = SanitizeXml(xmlPackContents);
            bool   packLoaded   = false;

            try {
                packDocument.LoadXml(packSrc);
                packLoaded = true;
            } catch (XmlException exception) {
                GameService.Debug.WriteErrorLine($"Could not load tacO overlay file {pathableResourceManager} from context {xmlPackContents.GetType().Name} due to an XML error.  Error: {exception.Message}");
            } catch (Exception ex) {
                throw;
            }

            if (packLoaded) {
                TryLoadCategories(packDocument);
                TryLoadPOIs(packDocument, pathableResourceManager, Categories);
            }
        }

        private static void TryLoadCategories(XmlDocument packDocument) {
            var categoryNodes = packDocument.DocumentElement?.SelectNodes("/OverlayData/MarkerCategory");
            if (categoryNodes == null) return;

            foreach (XmlNode categoryNode in categoryNodes) {
                PathingCategoryBuilder.UnpackCategory(categoryNode, Categories);
            }
        }

        private static void TryLoadPOIs(XmlDocument packDocument, PathableResourceManager pathableManager, PathingCategory rootCategory) {
            var poiNodes = packDocument.DocumentElement?.SelectSingleNode("/OverlayData/POIs");
            if (poiNodes == null) return;

            foreach (XmlNode poiNode in poiNodes) {
                PoiBuilder.UnpackPathable(poiNode, pathableManager, rootCategory);
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
