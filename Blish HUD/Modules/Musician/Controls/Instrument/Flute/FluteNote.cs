using System.Collections.Generic;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class FluteNote
    {
        public enum Keys
        {
            None,
            Note1,
            Note2,
            Note3,
            Note4,
            Note5,
            Note6,
            Note7,
            Note8
        }

        public enum Octaves
        {
            None,
            Low,
            High
        }

        private static readonly Dictionary<string, FluteNote> Map = new Dictionary<string, FluteNote>
        {
            {$"{Note.Keys.None}{Note.Octaves.None}", new FluteNote(Keys.None, Octaves.None)},

            // Low Octave
            {$"{Note.Keys.E}{Note.Octaves.Low}", new FluteNote(Keys.Note1, Octaves.Low)},
            {$"{Note.Keys.F}{Note.Octaves.Low}", new FluteNote(Keys.Note2, Octaves.Low)},
            {$"{Note.Keys.G}{Note.Octaves.Low}", new FluteNote(Keys.Note3, Octaves.Low)},
            {$"{Note.Keys.A}{Note.Octaves.Low}", new FluteNote(Keys.Note4, Octaves.Low)},
            {$"{Note.Keys.B}{Note.Octaves.Low}", new FluteNote(Keys.Note5, Octaves.Low)},
            {$"{Note.Keys.C}{Note.Octaves.Middle}", new FluteNote(Keys.Note6, Octaves.Low)},
            {$"{Note.Keys.D}{Note.Octaves.Middle}", new FluteNote(Keys.Note7, Octaves.Low)},
            {$"{Note.Keys.E}{Note.Octaves.Middle}", new FluteNote(Keys.Note8, Octaves.Low)},

            // High Octave
            {$"{Note.Keys.F}{Note.Octaves.Middle}", new FluteNote(Keys.Note2, Octaves.High)},
            {$"{Note.Keys.G}{Note.Octaves.Middle}", new FluteNote(Keys.Note3, Octaves.High)},
            {$"{Note.Keys.A}{Note.Octaves.Middle}", new FluteNote(Keys.Note4, Octaves.High)},
            {$"{Note.Keys.B}{Note.Octaves.Middle}", new FluteNote(Keys.Note5, Octaves.High)},

            // Highest Octave
            {$"{Note.Keys.C}{Note.Octaves.High}", new FluteNote(Keys.Note6, Octaves.High)},
            {$"{Note.Keys.D}{Note.Octaves.High}", new FluteNote(Keys.Note7, Octaves.High)},
            {$"{Note.Keys.E}{Note.Octaves.High}", new FluteNote(Keys.Note8, Octaves.High)}
        };

        public Keys Key { get; }
        public Octaves Octave { get; }

        public FluteNote(Keys key, Octaves octave)
        {
            Key = key;
            Octave = octave;
        }

        public static FluteNote From(Note note)
        {
            return Map[$"{note.Key}{note.Octave}"];
        }

        public override bool Equals(object obj)
        {
            return Equals((FluteNote) obj);
        }

        protected bool Equals(FluteNote other)
        {
            return Key == other.Key && Octave == other.Octave;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Key*397) ^ (int) Octave;
            }
        }
    }
}