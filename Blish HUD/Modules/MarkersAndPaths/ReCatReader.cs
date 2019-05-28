using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.MarkersAndPaths {

    public class ReCatSection {

        public string SectionName { get; }

        public List<string> Values { get; }

        public ReCatSection(string sectionName, List<string> sectionValues) {
            this.SectionName = sectionName;
            this.Values = sectionValues;
        }

    }

    public class ReCatReader {

        public static List<ReCatSection> FromFile(string filepath) {
            if (!File.Exists(filepath)) throw new FileNotFoundException($"Provided {nameof(filepath)} does not exist.", filepath);

            var loadedReCatSections = new List<ReCatSection>();

            string reCatContents = File.ReadAllText(filepath);

            using (var actualReCatReader = new StringReader(reCatContents)) {
                string currentSectionName = "Other.Uncategorized";
                var currentSectionValues = new List<string>();

                string lineContents;

                while ((lineContents = actualReCatReader.ReadLine()) != null) {
                    lineContents = lineContents.Trim();
                    if (lineContents.StartsWith("#")) continue; // Comments start with #, so ignore line
                    if (lineContents.StartsWith("[") && lineContents.EndsWith("]")) {
                        if (currentSectionValues.Count > 0) { // Package up and then clean up from the last section we loaded in
                            loadedReCatSections.Add(new ReCatSection(currentSectionName, currentSectionValues));
                        }

                        currentSectionName = lineContents.Substring(1, lineContents.Length - 2);
                        currentSectionValues = new List<string>();
                    } else if (lineContents.Length > 0) {
                        currentSectionValues.Add(lineContents);
                    }
                }
                if (currentSectionValues.Count > 0) { // Package up and then clean up from the last section in the file
                    loadedReCatSections.Add(new ReCatSection(currentSectionName, currentSectionValues));
                }
            }

            return loadedReCatSections;
        }

    }
}
