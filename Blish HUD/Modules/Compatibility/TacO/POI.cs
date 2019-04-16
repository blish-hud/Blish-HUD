using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD.Modules.Compatibility.TacO {
    public class POI : Pathing.Entities.Marker {

        public string Type { get; set; }
        public int Behavior { get; set; }
        public string GUID { get; protected set; }

        public override void Update(GameTime gameTime) {
            //if (this.Visible && this.FadeNear > 0 && this.FadeFar > 0) {
            //    this.Opacity = Math.Min(Math.Abs(this.DistanceFromPlayer - this.FadeNear) / Math.Abs(this.FadeFar - this.FadeNear), 1);
            //}

            //base.Update(gameTime);
        }

        public MarkerCategory ReferenceCategory;

        public POI(MarkerCategory referenceCategory, Vector3 position, string type, int behavior, string guid) : base(referenceCategory.IconTexture, position + new Vector3(0, 0, referenceCategory.HeightOffset), new Vector2(referenceCategory.IconSize * 2)) {
            ReferenceCategory = referenceCategory;
            this.Type = type;
            this.Behavior = behavior;
            this.GUID = guid;
            //this.FadeNear = referenceCategory.FadeNear;
            //this.FadeFar = referenceCategory.FadeFar;
        }

        public static POI FromXmlNode(XmlNode node) {
            int mapId = int.Parse(node.Attributes["MapID"]?.InnerText ?? "-1");
            float xPos = float.Parse(node.Attributes["xpos"]?.InnerText ?? "0");
            float yPos = float.Parse(node.Attributes["ypos"]?.InnerText ?? "0");
            float zPos = float.Parse(node.Attributes["zpos"]?.InnerText ?? "0");
            string type = node.Attributes["type"]?.InnerText.ToLower();
            int behavior = Utils.Pipeline.IntValueFromXmlNodeAttribute(node, "behavior");
            string guid = node.Attributes["GUID"]?.InnerText;

            // TODO: Check to make sure all necessary attributes are there before continuing to load the markers.
            // (Should probably have a list of required attributes to quickly check against)

            if (type == null) {
                return null;
            }

            var refCategory = OverlayData.MarkerCategoryFromPath(type);

            if (refCategory == null) {
                GameService.Debug.WriteErrorLine("'" + type + "' category was never defined!");
            }

            // Axis is swapped, so z and y must switch places.
            //var tacoPoint = new POI(refCategory, new Vector3(xPos, zPos, yPos), type, behavior, guid) {MapId = mapId};


            return null;
        }

    }
}
