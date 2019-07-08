namespace Blish_HUD.Modules.Musician.Domain.Values
{
    public class ChordOffset
    {
        public ChordOffset(Chord chord, Beat offest)
        {
            Chord = chord;
            Offest = offest;
        }

        public Chord Chord { get; }
        public Beat Offest { get; }
    }
}