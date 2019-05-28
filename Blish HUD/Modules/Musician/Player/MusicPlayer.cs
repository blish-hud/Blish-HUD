using System.Linq;
using System.Threading;
using Blish_HUD.Modules.Musician.Controls.Instrument;
using Blish_HUD.Modules.Musician.Domain;
using Blish_HUD.Modules.Musician.Player.Algorithms;
using Blish_HUD.Modules.Musician.Controls;
namespace Blish_HUD.Modules.Musician.Player
{
    public class MusicPlayer
    {
        public Thread Worker { get; private set; }
        public IPlayAlgorithm Algorithm { get; private set; }
        public void Dispose()
        {
            Algorithm.Dispose();
        }
        public MusicPlayer(MusicSheet musicSheet, InstrumentType instrument, IPlayAlgorithm algorithm)
        {
            Algorithm = algorithm;
            Worker = new Thread(() => algorithm.Play(instrument, musicSheet.MetronomeMark, musicSheet.Melody.ToArray()));
        }
    }
}