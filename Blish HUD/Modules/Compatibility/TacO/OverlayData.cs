using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Entities;
using Flurl.Util;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.Compatibility.TacO {

    public class OverlayData {

        public static Dictionary<string, MarkerCategory> OverlayCategories = new Dictionary<string, MarkerCategory>();
        public static void AppendCategories(MarkerCategory newCategory) {

            if (OverlayCategories.ContainsKey(newCategory.Name)) {
                foreach (var subCat in newCategory.SubCategories.Values) {
                    OverlayCategories[newCategory.Name].AppendCategories(subCat);
                }
            } else {
                OverlayCategories.Add(newCategory.Name, newCategory);
            }
        }

        public MarkerCategory Category { get; set; }
        
        public List<POI> POIs { get; set; }
        public List<TrailOld> Trails { get; set; }

        public OverlayData() {
            this.POIs = new List<POI>();
            this.Trails = new List<TrailOld>();
        }

        private static string SanitizeXml(string xmlDoc) {
            // TODO: Ask Tekkit to fix syntax
            return xmlDoc
                .Replace("=\"<", "=\"&lt;")
                .Replace(">\">", "&gt;\">")
                .Replace("*", "")
                .Replace("0behavior", "behavior");
        }

        public static OverlayData FromFile(string filepath) {
            if (!File.Exists(filepath)) return null;

            var tacoOverlayData = new OverlayData();

            var tpath = new XmlDocument();
            string overlaySrc = SanitizeXml(File.ReadAllText(filepath));

            try {
                tpath.LoadXml(overlaySrc);

                var categoryNode = tpath.DocumentElement?.SelectSingleNode("/OverlayData/MarkerCategory");
                if (categoryNode != null) {
                    var cNode = RecordMarkerCategory(categoryNode);
                    if (cNode != null)
                        AppendCategories(cNode);
                }


                var poisNode = tpath.DocumentElement.SelectSingleNode("/OverlayData/POIs");



                foreach (XmlNode poi in poisNode.ChildNodes) {
                    if (poi.Name == "POI") {
                        var nPOI = POI.FromXmlNode(poi);

                        if (nPOI != null) {
                            tacoOverlayData.POIs.Add(nPOI);
                            //GameService.Pathing.RegisterMarker(nPOI);
                        }
                    } else if (poi.Name == "Trail") {
                        // TODO: Trail needs to have a 'FromXmlNode' function just like POI does above to clean this all up
                        // TODO: Clean up texturePath (with .combine + system divider)
                        // TODO: Add better default trail texture if one is not provided
                        string texturePath = @"taco\" + poi.Attributes["texture"]?.InnerText;
                        var nText = GameService.Content.GetTexture("footsteps");
                        if (File.Exists(texturePath))
                            nText = Utils.Pipeline.TextureFromFile(GameService.Graphics.GraphicsDevice, texturePath);

                        var nTrail =
                            TrailOld.FromTrlFile(
                                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filepath), poi.Attributes["trailData"].InnerText),
                                nText);
                        nTrail.GUID = poi.Attributes["GUID"]?.InnerText;

                        if (nTrail != null) {
                            tacoOverlayData.Trails.Add(nTrail);
                        }
                    }
                }
        } catch (Exception exception) {
                GameService.Debug.WriteErrorLine("Could not load tacO overlay file {0}.  Error: {1}", filepath, exception.Message);
                return null;
            }

    GameService.Debug.WriteInfoLine("Successfully loaded tacO overlay file {0}!", filepath);

            return tacoOverlayData;
        }

        private static MarkerCategory RecordMarkerCategory(XmlNode markerCategoryNode) {
            var tacoMc = MarkerCategory.FromXmlNode(markerCategoryNode);

            if (tacoMc == null) return null;

            foreach (XmlNode category in markerCategoryNode.ChildNodes) {
                var ncNode = RecordMarkerCategory(category);

                if (ncNode != null) 
                    tacoMc.SubCategories.Add(ncNode.Name, ncNode);
            }

            return tacoMc;
        }

        public static MarkerCategory MarkerCategoryFromPath(string categoryPath) {
            string[] categories = categoryPath.Split('.');
            MarkerCategory current = null;

            foreach (string category in categories) {
                if (current == null) {
                    if (OverlayCategories.ContainsKey(category))
                        current = OverlayCategories[category];
                } else {
                    if (current.SubCategories.ContainsKey(category))
                        current = current.SubCategories[category];
                }
            }

            return current;
        }

    }
}
