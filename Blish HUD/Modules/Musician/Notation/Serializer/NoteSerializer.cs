using System;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Notation.Serializer
{
    public class NoteSerializer
    {
        public string Serialize(Note note)
        {
            // pause
            if (note.Key == Note.Keys.None)
                return "z";

            // low octave
            if (note.Key == Note.Keys.C && note.Octave == Note.Octaves.Lowest)
                return "C,";

            if (note.Key == Note.Keys.D && note.Octave == Note.Octaves.Lowest)
                return "D,";

            if (note.Key == Note.Keys.E && note.Octave == Note.Octaves.Lowest)
                return "E,";

            if (note.Key == Note.Keys.F && note.Octave == Note.Octaves.Lowest)
                return "F,";

            if (note.Key == Note.Keys.G && note.Octave == Note.Octaves.Lowest)
                return "G,";

            if (note.Key == Note.Keys.A && note.Octave == Note.Octaves.Lowest)
                return "A,";

            if (note.Key == Note.Keys.B && note.Octave == Note.Octaves.Lowest)
                return "B,";

            // middle octave
            if (note.Key == Note.Keys.C && note.Octave == Note.Octaves.Low)
                return "C";

            if (note.Key == Note.Keys.D && note.Octave == Note.Octaves.Low)
                return "D";

            if (note.Key == Note.Keys.E && note.Octave == Note.Octaves.Low)
                return "E";

            if (note.Key == Note.Keys.F && note.Octave == Note.Octaves.Low)
                return "F";

            if (note.Key == Note.Keys.G && note.Octave == Note.Octaves.Low)
                return "G";

            if (note.Key == Note.Keys.A && note.Octave == Note.Octaves.Low)
                return "A";

            if (note.Key == Note.Keys.B && note.Octave == Note.Octaves.Low)
                return "B";

            // high octave
            if (note.Key == Note.Keys.C && note.Octave == Note.Octaves.Middle)
                return "c";

            if (note.Key == Note.Keys.D && note.Octave == Note.Octaves.Middle)
                return "d";

            if (note.Key == Note.Keys.E && note.Octave == Note.Octaves.Middle)
                return "e";

            if (note.Key == Note.Keys.F && note.Octave == Note.Octaves.Middle)
                return "f";

            if (note.Key == Note.Keys.G && note.Octave == Note.Octaves.Middle)
                return "g";

            if (note.Key == Note.Keys.A && note.Octave == Note.Octaves.Middle)
                return "a";

            if (note.Key == Note.Keys.B && note.Octave == Note.Octaves.Middle)
                return "b";

            // highest octave
            if (note.Key == Note.Keys.C && note.Octave == Note.Octaves.High)
                return "c'";

            throw new NotSupportedException();
        }
    }
}