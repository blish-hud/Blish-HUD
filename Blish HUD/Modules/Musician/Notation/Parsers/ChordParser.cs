using System.Linq;
using System.Text.RegularExpressions;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Notation.Parsers
{
    public class ChordParser
    {
        private static readonly Regex NotesAndDurationRegex = new Regex(@"\[?([ABCDEFGZabcdefgz',]+)\]?(\d+)?\/?(\d+)?");
        private static readonly Regex NoteRegex = new Regex(@"([ABCDEFGZabcdefgz][,]{0,3}[']{0,2})");
        private readonly NoteParser _noteParser;
        private readonly string _instrument;

        public ChordParser(NoteParser noteParser, string instrument)
        {
            _noteParser = noteParser;
            _instrument = instrument;
        }

        public Chord Parse(string text)
        {
            var notesAndDuration = NotesAndDurationRegex.Match(text);

            var notes = notesAndDuration.Groups[1].Value;
            var nominator = notesAndDuration.Groups[2].Value;
            var denomintor = notesAndDuration.Groups[3].Value;

            var length = ParseFraction(nominator, denomintor);

            return new Chord(NoteRegex.Matches(notes)
                .Cast<Match>()
                .Select(x => _noteParser.Parse(x.Groups[1].Value, _instrument)),
                length);
        }

        private static Fraction ParseFraction(string nominator, string denominator)
        {
            return new Fraction(
                string.IsNullOrEmpty(nominator) ? 1 : int.Parse(nominator),
                string.IsNullOrEmpty(denominator) ? 1 : int.Parse(denominator));
        }
    }
}