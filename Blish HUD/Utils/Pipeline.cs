using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD.Utils {
    public static class Pipeline {

        public static Texture2D TextureFromFile(GraphicsDevice graphicsDevice, string filepath) {
            try {
                using (var fileStream = new FileStream(filepath, FileMode.Open)) {
                    return Texture2D.FromStream(graphicsDevice, fileStream);
                }
            } catch (System.IO.IOException ex) {
                GameService.Debug.WriteWarningLine($"Could not open file: {filepath}. {ex.Message}");
                return ContentService.Textures.Error;
            }
        }

        public static float FloatValueFromXmlNodeAttribute(XmlNode node, string attribute) {
            if (node.Attributes[attribute] != null) {
                float attrVal = 0;
                if (float.TryParse(node.Attributes[attribute].InnerText, out attrVal))
                    return attrVal;
            }
            return 0;
        }

        public static int IntValueFromXmlNodeAttribute(XmlNode node, string attribute) {
            if (node.Attributes[attribute] != null) {
                int attrVal = 0;
                if (int.TryParse(node.Attributes[attribute].InnerText, out attrVal))
                    return attrVal;
            }
            return 0;
        }

    }
}
