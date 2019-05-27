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

        public MusicPlayer(MusicSheet musicSheet, InstrumentType instrument, IPlayAlgorithm algorithm)
        {
            Worker = new Thread(() => algorithm.Play(instrument, musicSheet.MetronomeMark, musicSheet.Melody.ToArray()));
        }
    }
}