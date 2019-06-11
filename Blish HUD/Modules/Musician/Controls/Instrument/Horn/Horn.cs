using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Horn : Instrument
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<HornNote.Keys, GuildWarsControls> NoteMap = new Dictionary<HornNote.Keys, GuildWarsControls>
        {
            {HornNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {HornNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {HornNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {HornNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {HornNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {HornNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {HornNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {HornNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private HornNote.Octaves _currentOctave = HornNote.Octaves.Low;

        public Horn(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var hornNote = HornNote.From(note);

            if (RequiresAction(hornNote))
            {
                if (hornNote.Key == HornNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    hornNote = OptimizeNote(hornNote);
                    PressNote(NoteMap[hornNote.Key]);
                }
            }
        }

        public override void GoToOctave(Note note)
        {
            var hornNote = HornNote.From(note);

            if (RequiresAction(hornNote))
            {
                hornNote = OptimizeNote(hornNote);

                while (_currentOctave != hornNote.Octave)
                {
                    if (_currentOctave < hornNote.Octave)
                    {
                        IncreaseOctave();
                    }
                    else
                    {
                        DecreaseOctave();
                    }
                }
            }
        }

        private static bool RequiresAction(HornNote hornNote)
        {
            return hornNote.Key != HornNote.Keys.None;
        }

        private HornNote OptimizeNote(HornNote note)
        {
            if (note.Equals(new HornNote(HornNote.Keys.Note1, HornNote.Octaves.High)) && _currentOctave == HornNote.Octaves.Middle)
            {
                note = new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Middle);
            }
            else if (note.Equals(new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Middle)) && _currentOctave == HornNote.Octaves.High)
            {
                note = new HornNote(HornNote.Keys.Note1, HornNote.Octaves.High);
            }
            else if (note.Equals(new HornNote(HornNote.Keys.Note1, HornNote.Octaves.Middle)) && _currentOctave == HornNote.Octaves.Low)
            {
                note = new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Low);
            }
            else if (note.Equals(new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Low)) && _currentOctave == HornNote.Octaves.Middle)
            {
                note = new HornNote(HornNote.Keys.Note1, HornNote.Octaves.Middle);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case HornNote.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToMiddle;
                    _currentOctave = HornNote.Octaves.Middle;
                    break;
                case HornNote.Octaves.Middle:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = HornNote.Octaves.High;
                    break;
                case HornNote.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _keyboard.Press(GuildWarsControls.EliteSkill);
            _keyboard.Release(GuildWarsControls.EliteSkill);

            Thread.Sleep(OctaveTimeout);
        }

        private void DecreaseOctave()
        {
            var noteType = InstrumentSkillType.DecreaseOctaveToLow;
            switch (_currentOctave)
            {
                case HornNote.Octaves.Low:
                    break;
                case HornNote.Octaves.Middle:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = HornNote.Octaves.Low;
                    break;
                case HornNote.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToMiddle;
                    _currentOctave = HornNote.Octaves.Middle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _keyboard.Press(GuildWarsControls.UtilitySkill3);
            _keyboard.Release(GuildWarsControls.UtilitySkill3);

            Thread.Sleep(OctaveTimeout);
        }

        private void PressNote(GuildWarsControls key)
        {
            var noteType = InstrumentSkillType.MiddleNote;
            switch (_currentOctave)
            {
                case HornNote.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case HornNote.Octaves.Middle:
                    noteType = InstrumentSkillType.MiddleNote;
                    break;
                case HornNote.Octaves.High:
                    noteType = InstrumentSkillType.HighNote;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _keyboard.Press(key);
            _keyboard.Release(key);

            Thread.Sleep(NoteTimeout);
        }
    }
}