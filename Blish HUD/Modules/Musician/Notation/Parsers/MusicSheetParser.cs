using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blish_HUD.Modules.Musician.Domain;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Notation.Parsers
{
    public class MusicSheetParser
    {
        private static readonly Regex NonWhitespace = new Regex(@"[^\s]+");
        private readonly ChordParser _chordParser;

        public MusicSheetParser(ChordParser chordParser)
        {
            _chordParser = chordParser;
        }

        public MusicSheet Parse(string text, int metronome, int nominator, int denominator)
        {
            var beatsPerMeasure = BeatsPerMeasure(nominator, denominator);

            return new MusicSheet(string.Empty, string.Empty, new MetronomeMark(metronome, beatsPerMeasure), ParseMelody(text));
        }

        private static Fraction BeatsPerMeasure(int nominator, int denominator)
        {
            return new Fraction(nominator, denominator);
        }

        private IEnumerable<ChordOffset> ParseMelody(string textMelody)
        {
            var currentBeat = 0m;

            return NonWhitespace.Matches(textMelody)
                .Cast<Match>()
                .Select(textChord =>
                {
                    var chord = _chordParser.Parse(textChord.Value);

                    var chordOffset = new ChordOffset(chord, new Beat(currentBeat));

                    currentBeat += chord.Length;

                    return chordOffset;
                });
        }
    }
}