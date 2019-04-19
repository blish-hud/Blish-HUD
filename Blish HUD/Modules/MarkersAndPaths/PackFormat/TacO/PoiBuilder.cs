using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Blish_HUD.Modules.Compatibility.TacO;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Pathables;
using Blish_HUD.Pathing;
using Microsoft.Scripting.Utils;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat {
    /*
     *# Size
     *# Alpha
     *# FadeNear
     *# FadeFar
     *# Height
     *# Behavior
     * ResetLength
     * ResetOffset
     * AutoTrigger
     * HasCountdown
     * TriggerRange
     *# MinSize
     *# MaxSize
     * Color
     * TrailData
     * AnimSpeed
     * Texture
     * TrailScale
     * ToggleCategory
     */

    public class TacOPathable {

        public float Size { get; set; }
        public float Alpha { get; set; }
        public float FadeNear { get; set; }
        public float FadeFar { get; set; }
        public float HeightOffset { get; set; }
        public int Behavior { get; set; }
        public int ResetLength { get; set; }
        public int ResetOffset { get; set; }
        public int AutoTrigger { get; set; }
        public int HasCountdown { get; set; }
        public float TriggerRange { get; set; }
        public int MinSize { get; set; }
        public int MaxSize { get; set; }
        public float AnimSpeed { get; set; }
        public int MapId { get; set; }
        public string Texture { get; set; }
        public float XPos { get; set; }
        public float YPos { get; set; }
        public float ZPos { get; set; }
        public string Type { get; set; }
        public string Guid { get; set; }

    }



    public static class PoiBuilder {

        private const string ELEMENT_POITYPE_POI = "poi";
        private const string ELEMENT_POITYPE_TRAIL = "trail";
        private const string ELEMENT_POITYPE_ROUTE = "route";

        public static void UnpackPathable(XmlNode pathableNode, IPackFileSystemContext packContext) {
            switch (pathableNode.Name.ToLower()) {
                case ELEMENT_POITYPE_POI:
                    var newPoiMarker = new TacOMarkerPathable(pathableNode, packContext);

                    if (newPoiMarker.SuccessfullyLoaded) {
                        GameService.Pathing.RegisterPathable(newPoiMarker);
                    } else {
                        Console.WriteLine("Failed to load marker: ");
                        Console.WriteLine(string.Join("; ", pathableNode.Attributes.Select(s => ((XmlAttribute)s).Name + " = " + ((XmlAttribute)s).Value)));
                    }
                    break;
                case ELEMENT_POITYPE_TRAIL:
                    var newPathTrail = new TacOTrailPathable(pathableNode, packContext);

                    if (newPathTrail.SuccessfullyLoaded) {
                        GameService.Pathing.RegisterPathable(newPathTrail);
                    } else {
                        Console.WriteLine("Failed to load trail: ");
                        Console.WriteLine(string.Join("; ", pathableNode.Attributes.Select(s => ((XmlAttribute)s).Name + " = " + ((XmlAttribute)s).Value)));
                    }

                    break;
                case ELEMENT_POITYPE_ROUTE:
                    Console.WriteLine("Skipped loading route.");
                    //RouteBuilder.UnpackNode(pathableNode);

                    break;
                default:
                    Console.WriteLine($"Tried to unpack '{pathableNode.Name}' as POI!");
                    break;
            }
        }

    }

}
