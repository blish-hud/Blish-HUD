using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Microsoft.Xna.Framework;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public enum InstrumentSkillType
    {
        None,
        LowNote,
        MiddleNote,
        HighNote,
        IncreaseOctaveToMiddle,
        IncreaseOctaveToHigh,
        DecreaseOctaveToLow,
        DecreaseOctaveToMiddle,
        StopPlaying
    }
    public class InstrumentType
    {
        public InstrumentType() { }
        public virtual void PlayNote(Note note) { }
        public virtual void GoToOctave(Note note) { }
    }
}