using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD {
    public static class XmlNodeExtensions {

        public static string ToString(this XmlNode node, int indentation) {
            using (var sw = new StringWriter()) {
                using (var xw = new XmlTextWriter(sw)) {
                    xw.Formatting  = Formatting.Indented;
                    xw.Indentation = indentation;
                    node.WriteContentTo(xw);
                }
                return sw.ToString();
            }
        }

    }
}
