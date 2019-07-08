namespace Blish_HUD.Modules.Musician.Domain.Values
{
    public class Note
    {
        public enum Keys
        {
            None,
            C,
            D,
            E,
            F,
            G,
            A,
            B
        }

        public enum Octaves
        {
            None,
            Lowest,
            Low,
            Middle,
            High,
            Highest
        }

        public Note(Keys key, Octaves octave)
        {
            Key = key;
            Octave = octave;
        }

        public Keys Key { get; }
        public Octaves Octave { get; }

        public override string ToString()
        {
            return $"{(Octave >= Octaves.High ? "▲" : Octave <= Octaves.Low ? "▼" : string.Empty)}{Key}";
        }

        public override bool Equals(object obj)
        {
            return Equals((Note) obj);
        }

        protected bool Equals(Note other)
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