using System.Linq;
using System.Text;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Notation.Serializer
{
    public class ChordSerializer
    {
        private readonly bool _includeChordDuration;
        private readonly NoteSerializer _noteSerializer;

        public ChordSerializer(NoteSerializer noteSerializer, bool includeChordDuration)
        {
            _includeChordDuration = includeChordDuration;
            _noteSerializer = noteSerializer;
        }

        public string Serialize(Chord chord)
        {
            var stringBuilder = new StringBuilder();

            if (chord.Notes.Count() > 1)
            {
                stringBuilder.Append("[");
            }

            foreach (var note in chord.Notes)
            {
                stringBuilder.Append(_noteSerializer.Serialize(note));
            }

            if (chord.Notes.Count() > 1)
            {
                stringBuilder.Append("]");
            }

            if (_includeChordDuration)
            {
                if (chord.Length.Nominator != 1)
                {
                    stringBuilder.Append(chord.Length.Nominator);
                }

                if (chord.Length.Denominator != 1)
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(chord.Length.Denominator);
                }
            }

            return stringBuilder.ToString();
        }
    }
}