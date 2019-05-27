using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
namespace Blish_HUD.Modules.Musician.Notation.Persistance
{
    public class XmlMusicSheetReader
    {
        public List<RawMusicSheet> cachedSheets = new List<RawMusicSheet>();
        public string[] loadedSheets;
        public RawMusicSheet LoadFromFile(string path)
        {
            var xDocument = XDocument.Load(path);

            return new RawMusicSheet(
                xDocument.Elements().Single().Elements("artist").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("title").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("user").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("instrument").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("tempo").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("meter").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("melody").SingleOrDefault()?.Value,
                xDocument.Elements().Single().Elements("algorithm").SingleOrDefault()?.Value
            );
        }
        public List<RawMusicSheet> LoadDirectory(string path)
        {
            cachedSheets.Clear();
            loadedSheets = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string file in loadedSheets)
            {
                cachedSheets.Add(LoadFromFile(file));
            }
            return cachedSheets;
        }
    }
}