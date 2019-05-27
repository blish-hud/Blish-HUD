using System.Text;
using Blish_HUD.Modules.Musician.Domain.Values;

namespace Blish_HUD.Modules.Musician.Notation.Serializer
{
    public class ChordOffsetSerializer
    {
        private readonly ChordSerializer _chordSerializer;

        public ChordOffsetSerializer(ChordSerializer chordSerializer)
        {
            _chordSerializer = chordSerializer;
        }

        public string Serialize(ChordOffset chordOffset)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_chordSerializer.Serialize(chordOffset.Chord));

            return stringBuilder.ToString();
        }
    }
}