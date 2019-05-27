using System;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Notation.Parsers
{
    public class NoteParser
    {
        public Note Parse(string text, string instrument)
        {
            var key = ParseKey(text);
            var octave = ParseOctave(text, instrument);

            return new Note(key, octave);
        }

        private static Note.Keys ParseKey(string text)
        {
            Note.Keys key;
            switch (text)
            {
                case "Z,":
                case "Z,,":
                case "Z,,,":
                case "Z":
                case "z":
                case "z'":
                    key = Note.Keys.None;
                    break;
                case "C,":
                case "C,,":
                case "C,,,":
                case "C":
                case "c":
                case "c'":
                case "c''":
                    key = Note.Keys.C;
                    break;
                case "D,":
                case "D,,":
                case "D,,,":
                case "D":
                case "d":
                case "d'":
                case "d''":
                    key = Note.Keys.D;
                    break;
                case "E,":
                case "E,,":
                case "E,,,":
                case "E":
                case "e":
                case "e'":
                    key = Note.Keys.E;
                    break;
                case "F,":
                case "F,,":
                case "F,,,":
                case "F":
                case "f":
                case "f'":
                    key = Note.Keys.F;
                    break;
                case "G,":
                case "G,,":
                case "G,,,":
                case "G":
                case "g":
                case "g'":
                    key = Note.Keys.G;
                    break;
                case "A,":
                case "A,,":
                case "A,,,":
                case "A":
                case "a":
                case "a'":
                    key = Note.Keys.A;
                    break;
                case "B,":
                case "B,,":
                case "B,,,":
                case "B":
                case "b":
                case "b'":
                    key = Note.Keys.B;
                    break;
                default:
                    throw new NotSupportedException(text);
            }
            return key;
        }

        private static Note.Octaves ParseOctave(string text, string instrument)
        {
            switch (text)
            {
                case "Z,":
                case "Z":
                case "z":
                case "z'":
                    return Note.Octaves.None;
                case "C,":
                case "D,":
                case "E,":
                case "F,":
                case "G,":
                case "A,":
                case "B,":
                    return Note.Octaves.Lowest;
                case "C":
                case "D":
                case "E":
                case "F":
                case "G":
                case "A":
                case "B":
                case "C,,":
                case "D,,":
                case "E,,":
                case "F,,":
                case "G,,":
                case "A,,":
                case "B,,":
                    return Note.Octaves.Low;
                case "c":
                case "d":
                case "e":
                case "f":
                case "g":
                case "a":
                case "b":
                case "C,,,":
                case "D,,,":
                case "E,,,":
                case "F,,,":
                case "G,,,":
                case "A,,,":
                case "B,,,":
                    return Note.Octaves.Middle;
                case "c'":
                case "d'":
                case "e'":
                case "f'":
                case "g'":
                case "a'":
                case "b'":
                    return Note.Octaves.High;
                case "c''":
                case "d''":
                    return Note.Octaves.Highest;
                default:
                    throw new NotSupportedException(text);
            }
        }
    }
}