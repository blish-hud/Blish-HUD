using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat {
    public static class PathingCategoryBuilder {

        private const string ELEMENT_CATEGORY = "MarkerCategory";

        public static void UnpackCategory(XmlNode categoryNode, PathingCategory categoryParent) {
            if (categoryNode.Name != ELEMENT_CATEGORY) {
                Console.WriteLine($"Tried to unpack '{categoryNode.Name}' as category!");
                return;
            }

            var loadedCategory = FromXmlNode(categoryNode, categoryParent);

            if (loadedCategory == null) return;

            foreach (XmlNode childCategoryNode in categoryNode) {
                UnpackCategory(childCategoryNode, loadedCategory);
            }
        }

        public static PathingCategory FromXmlNode(XmlNode categoryNode, PathingCategory parent) {
            string categoryName = categoryNode.Attributes["name"]?.InnerText.ToLower();

            // Can't define a marker category without a name
            if (string.IsNullOrEmpty(categoryName)) return null;

            var subjCategory = parent.Contains(categoryName)
                                   // We're extending an existing category
                                   ? parent[categoryName]
                                   // We're adding a new category
                                   : parent.GetOrAddCategoryFromNamespace(categoryName);

            subjCategory.DisplayName = categoryNode.Attributes["DisplayName"]?.InnerText; // ?? categoryNode.Attributes["name"]?.InnerText;
            //subjCategory.IconFile    = categoryNode.Attributes["iconFile"]?.InnerText.Replace('/', System.IO.Path.DirectorySeparatorChar);

            subjCategory.SourceXmlNode = categoryNode;

            //float.TryParse(categoryNode.Attributes["iconSize"]?.InnerText,     out subjCategory.Size);
            //float.TryParse(categoryNode.Attributes["heightOffset"]?.InnerText, out subjCategory.Height);
            //float.TryParse(categoryNode.Attributes["fadeFar"]?.InnerText,      out subjCategory.FadeFar);
            //float.TryParse(categoryNode.Attributes["FadeStart"]?.InnerText,    out subjCategory.FadeNear);
            //float.TryParse(categoryNode.Attributes["alpha"]?.InnerText,        out subjCategory.Alpha);

            return subjCategory;
        }

    }
}
