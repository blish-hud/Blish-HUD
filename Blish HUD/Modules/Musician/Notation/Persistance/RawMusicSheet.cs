namespace Blish_HUD.Modules.Musician.Notation.Persistance
{
    public class RawMusicSheet
    {
        public RawMusicSheet(string artist, string title, string user, string instrument, string tempo, string meter, string melody, string algorithm)
        {
            Artist = artist;
            Title = title;
            User = user;
            Instrument = instrument.ToLower();
            Tempo = tempo;
            Melody = melody;
            Algorithm = algorithm.ToLower();
            Meter = meter;
        }
        public string Artist { get; private set; }
        public string Title { get; private set; }
        public string User { get; private set; }
        public string Instrument { get; private set; }
        public string Tempo { get; private set; }
        public string Melody { get; private set; }
        public string Meter { get; private set; }
        public string Algorithm { get; private set; }
    }
}