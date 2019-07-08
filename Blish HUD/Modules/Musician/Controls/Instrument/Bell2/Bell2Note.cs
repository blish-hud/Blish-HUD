using System.Collections.Generic;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Bell2Note
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

        private static readonly Dictionary<string, Bell2Note> Map = new Dictionary<string, Bell2Note>
        {
            {$"{Note.Keys.None}{Note.Octaves.None}", new Bell2Note(Keys.None, Octaves.None)},

            // Low Octave
            {$"{Note.Keys.C}{Note.Octaves.Middle}", new Bell2Note(Keys.Note1, Octaves.Low)},
            {$"{Note.Keys.D}{Note.Octaves.Middle}", new Bell2Note(Keys.Note2, Octaves.Low)},
            {$"{Note.Keys.E}{Note.Octaves.Middle}", new Bell2Note(Keys.Note3, Octaves.Low)},
            {$"{Note.Keys.F}{Note.Octaves.Middle}", new Bell2Note(Keys.Note4, Octaves.Low)},
            {$"{Note.Keys.G}{Note.Octaves.Middle}", new Bell2Note(Keys.Note5, Octaves.Low)},
            {$"{Note.Keys.A}{Note.Octaves.Middle}", new Bell2Note(Keys.Note6, Octaves.Low)},
            {$"{Note.Keys.B}{Note.Octaves.Middle}", new Bell2Note(Keys.Note7, Octaves.Low)},
            //{$"{Note.Keys.C}{Note.Octaves.High}", new Bell2Note(Keys.Note8, Octaves.Low)}, // Note to optimize at runtime.
            // High Octave
            {$"{Note.Keys.C}{Note.Octaves.High}", new Bell2Note(Keys.Note1, Octaves.High)},
            {$"{Note.Keys.D}{Note.Octaves.High}", new Bell2Note(Keys.Note2, Octaves.High)},
            {$"{Note.Keys.E}{Note.Octaves.High}", new Bell2Note(Keys.Note3, Octaves.High)},
            {$"{Note.Keys.F}{Note.Octaves.High}", new Bell2Note(Keys.Note4, Octaves.High)},
            {$"{Note.Keys.G}{Note.Octaves.High}", new Bell2Note(Keys.Note5, Octaves.High)},
            {$"{Note.Keys.A}{Note.Octaves.High}", new Bell2Note(Keys.Note6, Octaves.High)},
            {$"{Note.Keys.B}{Note.Octaves.High}", new Bell2Note(Keys.Note7, Octaves.High)},
            {$"{Note.Keys.C}{Note.Octaves.Highest}", new Bell2Note(Keys.Note8, Octaves.High)}
        };

        public Keys Key { get; }
        public Octaves Octave { get; }

        public Bell2Note(Keys key, Octaves octave)
        {
            Key = key;
            Octave = octave;
        }

        public static Bell2Note From(Note note)
        {
            return Map[$"{note.Key}{note.Octave}"];
        }

        public override bool Equals(object obj)
        {
            return Equals((Bell2Note) obj);
        }

        protected bool Equals(Bell2Note other)
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