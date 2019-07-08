using System.Collections.Generic;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class BassNote
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

        private static readonly Dictionary<string, BassNote> Map = new Dictionary<string, BassNote>
        {
            {$"{Note.Keys.None}{Note.Octaves.None}", new BassNote(Keys.None, Octaves.None)},

            // Low Octave
            {$"{Note.Keys.C}{Note.Octaves.Middle}", new BassNote(Keys.Note1, Octaves.Low)},
            {$"{Note.Keys.D}{Note.Octaves.Middle}", new BassNote(Keys.Note2, Octaves.Low)},
            {$"{Note.Keys.E}{Note.Octaves.Middle}", new BassNote(Keys.Note3, Octaves.Low)},
            {$"{Note.Keys.F}{Note.Octaves.Middle}", new BassNote(Keys.Note4, Octaves.Low)},
            {$"{Note.Keys.G}{Note.Octaves.Middle}", new BassNote(Keys.Note5, Octaves.Low)},
            {$"{Note.Keys.A}{Note.Octaves.Middle}", new BassNote(Keys.Note6, Octaves.Low)},
            {$"{Note.Keys.B}{Note.Octaves.Middle}", new BassNote(Keys.Note7, Octaves.Low)},

            // High Octave
            {$"{Note.Keys.C}{Note.Octaves.Low}", new BassNote(Keys.Note1, Octaves.High)},
            {$"{Note.Keys.D}{Note.Octaves.Low}", new BassNote(Keys.Note2, Octaves.High)},
            {$"{Note.Keys.E}{Note.Octaves.Low}", new BassNote(Keys.Note3, Octaves.High)},
            {$"{Note.Keys.F}{Note.Octaves.Low}", new BassNote(Keys.Note4, Octaves.High)},
            {$"{Note.Keys.G}{Note.Octaves.Low}", new BassNote(Keys.Note5, Octaves.High)},
            {$"{Note.Keys.A}{Note.Octaves.Low}", new BassNote(Keys.Note6, Octaves.High)},
            {$"{Note.Keys.B}{Note.Octaves.Low}", new BassNote(Keys.Note7, Octaves.High)},
            {$"{Note.Keys.C}{Note.Octaves.Lowest}", new BassNote(Keys.Note8, Octaves.High)}
        };

        public Keys Key { get; }
        public Octaves Octave { get; }

        public BassNote(Keys key, Octaves octave)
        {
            Key = key;
            Octave = octave;
        }

        public static BassNote From(Note note)
        {
            return Map[$"{note.Key}{note.Octave}"];
        }

        public override bool Equals(object obj)
        {
            return Equals((BassNote) obj);
        }

        protected bool Equals(BassNote other)
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