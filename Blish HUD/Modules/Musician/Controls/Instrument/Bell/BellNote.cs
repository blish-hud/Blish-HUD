using System.Collections.Generic;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class BellNote
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
            Middle,
            High
        }

        private static readonly Dictionary<string, BellNote> Map = new Dictionary<string, BellNote>
        {
            {$"{Note.Keys.None}{Note.Octaves.None}", new BellNote(Keys.None, Octaves.None)},

            // Low Octave
            {$"{Note.Keys.D}{Note.Octaves.Low}", new BellNote(Keys.Note1, Octaves.Low)},
            {$"{Note.Keys.E}{Note.Octaves.Low}", new BellNote(Keys.Note2, Octaves.Low)},
            {$"{Note.Keys.F}{Note.Octaves.Low}", new BellNote(Keys.Note3, Octaves.Low)},
            {$"{Note.Keys.G}{Note.Octaves.Low}", new BellNote(Keys.Note4, Octaves.Low)},
            {$"{Note.Keys.A}{Note.Octaves.Low}", new BellNote(Keys.Note5, Octaves.Low)},
            {$"{Note.Keys.B}{Note.Octaves.Low}", new BellNote(Keys.Note6, Octaves.Low)},
            {$"{Note.Keys.C}{Note.Octaves.Middle}", new BellNote(Keys.Note7, Octaves.Low)},

            //{$"{Note.Keys.D}{Note.Octaves.Middle}", new BellNote(Keys.Note8, Octaves.Low)}, // Note to optimize at runtime.
            // Middle Octave
            {$"{Note.Keys.D}{Note.Octaves.Middle}", new BellNote(Keys.Note1, Octaves.Middle)},
            {$"{Note.Keys.E}{Note.Octaves.Middle}", new BellNote(Keys.Note2, Octaves.Middle)},
            {$"{Note.Keys.F}{Note.Octaves.Middle}", new BellNote(Keys.Note3, Octaves.Middle)},
            {$"{Note.Keys.G}{Note.Octaves.Middle}", new BellNote(Keys.Note4, Octaves.Middle)},
            {$"{Note.Keys.A}{Note.Octaves.Middle}", new BellNote(Keys.Note5, Octaves.Middle)},
            {$"{Note.Keys.B}{Note.Octaves.Middle}", new BellNote(Keys.Note6, Octaves.Middle)},
            {$"{Note.Keys.C}{Note.Octaves.High}", new BellNote(Keys.Note7, Octaves.Middle)},

            //{$"{Note.Keys.D}{Note.Octaves.High}", new BellNote(Keys.Note8, Octaves.Middle)}, // Note to optimize at runtime.
            // High Octave
            {$"{Note.Keys.D}{Note.Octaves.High}", new BellNote(Keys.Note1, Octaves.High)},
            {$"{Note.Keys.E}{Note.Octaves.High}", new BellNote(Keys.Note2, Octaves.High)},
            {$"{Note.Keys.F}{Note.Octaves.High}", new BellNote(Keys.Note3, Octaves.High)},
            {$"{Note.Keys.G}{Note.Octaves.High}", new BellNote(Keys.Note4, Octaves.High)},
            {$"{Note.Keys.A}{Note.Octaves.High}", new BellNote(Keys.Note5, Octaves.High)},
            {$"{Note.Keys.B}{Note.Octaves.High}", new BellNote(Keys.Note6, Octaves.High)},

            {$"{Note.Keys.C}{Note.Octaves.Highest}", new BellNote(Keys.Note7, Octaves.High)},
            {$"{Note.Keys.D}{Note.Octaves.Highest}", new BellNote(Keys.Note8, Octaves.High)},
        };

        public Keys Key { get; }
        public Octaves Octave { get; }

        public BellNote(Keys key, Octaves octave)
        {
            Key = key;
            Octave = octave;
        }

        public static BellNote From(Note note)
        {
            return Map[$"{note.Key}{note.Octave}"];
        }

        public override bool Equals(object obj)
        {
            return Equals((BellNote) obj);
        }

        protected bool Equals(BellNote other)
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