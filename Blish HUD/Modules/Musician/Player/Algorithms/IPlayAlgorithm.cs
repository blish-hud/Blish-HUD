using Blish_HUD.Modules.Musician.Controls.Instrument;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Modules.Musician.Controls;
namespace Blish_HUD.Modules.Musician.Player.Algorithms
{
    public interface IPlayAlgorithm
    {
        void Play(InstrumentType instrument, MetronomeMark metronomeMark, ChordOffset[] melody);
    }
}