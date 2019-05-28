using System.IO;
using System.Xml.Linq;

namespace Blish_HUD.Modules.Musician.Notation.Persistance
{
    public class XmlMusicSheetWriter
    {
        public void SaveToFile(RawMusicSheet musicSheet, string path)
        {
            var xDocument = new XDocument(
                new XElement("song",
                    new XElement("artist", musicSheet.Artist),
                    new XElement("title", musicSheet.Title),
                    new XElement("user", musicSheet.User),
                    new XElement("instrument", musicSheet.Instrument),
                    new XElement("tempo", musicSheet.Tempo),
                    new XElement("meter", musicSheet.Meter),
                    new XElement("algorithm", musicSheet.Algorithm),
                    new XElement("melody", musicSheet.Melody))
                );

            using (var fileStream = File.Open(path, FileMode.Create))
            {
                xDocument.Save(fileStream);
            }
        }
    }
}