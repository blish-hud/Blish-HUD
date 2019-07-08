using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO {
    public static class RouteBuilder {

        private const string ELEMENT_POITYPE_ROUTE = "route";

        public static void UnpackNode(XmlNode routeNode) {
            if (routeNode.Name.ToLower() != ELEMENT_POITYPE_ROUTE)
                throw new ArgumentException($"{nameof(routeNode)} is not an '{ELEMENT_POITYPE_ROUTE}' XML element.", nameof(routeNode));

            //var newRoute = FromXmlNode(routeNode);

            //if (newRoute != null)
            //    GameService.Graphics.World.Entities.Add(newRoute);
                //GameService.Pathing.RegisterPath(newRoute);
        }

        //public static Blish_HUD.Entities.Route FromXmlNode(XmlNode routeNode) {
            //var routeMarkers = new List<Blish_HUD.Entities.Marker>();

            //foreach (XmlNode poiNode in routeNode) {
            //    routeMarkers.Add(PoiBuilder.FromXmlNode(poiNode));
            //}

            //return routeMarkers.Any() ? new Route(routeMarkers) : null;

        //}

    }
}
