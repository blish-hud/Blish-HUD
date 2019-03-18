using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Blish_HUD.Modules.Compatibility.TacO;
using Microsoft.Xna.Framework;
using Praeclarum.Bind;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat {

    public static class PoiBuilder {

        private const string ELEMENT_POITYPE_POI = "poi";
        private const string ELEMENT_POITYPE_TRAIL = "trail";

        public static void UnpackPoi(XmlNode poiNode) {
            switch (poiNode.Name.ToLower()) {
                case ELEMENT_POITYPE_POI:
                    var newMarker = FromXmlNode(poiNode);

                    if (newMarker != null) {
                        GameService.Pathing.RegisterMarker(newMarker);
                    }

                    break;
                case ELEMENT_POITYPE_TRAIL:
                    var newTrails = Entities.Paths.Trail.FromXmlNode(poiNode);

                    newTrails.ForEach((trail) => GameService.Pathing.RegisterPath(trail));

                    break;
                default:
                    Console.WriteLine($"Tried to unpack '{poiNode.Name}' as POI!");
                    break;
            }
        }

        public static Blish_HUD.Entities.Marker FromXmlNode(XmlNode poiNode) {
            int mapId = int.Parse(poiNode.Attributes["MapID"]?.InnerText ?? "-1");

            float xPos = float.Parse(poiNode.Attributes["xpos"]?.InnerText ?? "0");
            float yPos = float.Parse(poiNode.Attributes["ypos"]?.InnerText ?? "0");
            float zPos = float.Parse(poiNode.Attributes["zpos"]?.InnerText ?? "0");

            string type = poiNode.Attributes["type"]?.InnerText;

            int behavior = Utils.Pipeline.IntValueFromXmlNodeAttribute(poiNode, "behavior");

            string guid = poiNode.Attributes["GUID"]?.InnerText;

            // type is required
            if (type == null) return null;

            var refCategory = GameService.Pathing.Categories.GetOrAddCategoryFromNamespace(type);

            var loadedMarker = new Blish_HUD.Entities.Marker(
                                       GameService.Content.GetTexture(
                                                                      System.IO.Path.Combine(
                                                                                             GameService.FileSrv.BasePath,
                                                                                             MarkersAndPaths.MARKER_DIRECTORY,
                                                                                             refCategory.IconFile ?? ""
                                                                                            )
                                                                     ),
                                       new Vector3(xPos, zPos, yPos),
                                       new Vector2(1)
                                      ) { MapId = mapId };

            // Ensure the marker state matches that of the assigned category
            Binding.Create(() => loadedMarker.Visible == refCategory.Visible);
            //loadedMarker.Visible = true;

            return loadedMarker;
        }

    }
}
