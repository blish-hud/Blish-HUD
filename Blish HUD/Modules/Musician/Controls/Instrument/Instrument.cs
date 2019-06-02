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
    public abstract class Instrument
    {
        public bool IsInstrument(string instrument) {
            return this.GetType().Name.ToLower().Trim() == instrument.ToLower().Trim();
        }
        public abstract void PlayNote(Note note);
        public abstract void GoToOctave(Note note);
    }
}