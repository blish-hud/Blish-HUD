using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Lute : InstrumentType
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<LuteNote.Keys, GuildWarsControls> NoteMap = new Dictionary<LuteNote.Keys, GuildWarsControls>
        {
            {LuteNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {LuteNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {LuteNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {LuteNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {LuteNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {LuteNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {LuteNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {LuteNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private LuteNote.Octaves _currentOctave = LuteNote.Octaves.Low;

        public Lute(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var luteNote = LuteNote.From(note);

            if (RequiresAction(luteNote))
            {
                if (luteNote.Key == LuteNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    luteNote = OptimizeNote(luteNote);
                    PressNote(NoteMap[luteNote.Key]);
                }
            }
        }

        public override void GoToOctave(Note note)
        {
            var luteNote = LuteNote.From(note);

            if (RequiresAction(luteNote))
            {
                luteNote = OptimizeNote(luteNote);

                while (_currentOctave != luteNote.Octave)
                {
                    if (_currentOctave < luteNote.Octave)
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

        private static bool RequiresAction(LuteNote luteNote)
        {
            return luteNote.Key != LuteNote.Keys.None;
        }

        private LuteNote OptimizeNote(LuteNote note)
        {
            if (note.Equals(new LuteNote(LuteNote.Keys.Note1, LuteNote.Octaves.High)) && _currentOctave == LuteNote.Octaves.Middle)
            {
                note = new LuteNote(LuteNote.Keys.Note8, LuteNote.Octaves.Middle);
            }
            else if (note.Equals(new LuteNote(LuteNote.Keys.Note8, LuteNote.Octaves.Middle)) && _currentOctave == LuteNote.Octaves.High)
            {
                note = new LuteNote(LuteNote.Keys.Note1, LuteNote.Octaves.High);
            }
            else if (note.Equals(new LuteNote(LuteNote.Keys.Note1, LuteNote.Octaves.Middle)) && _currentOctave == LuteNote.Octaves.Low)
            {
                note = new LuteNote(LuteNote.Keys.Note8, LuteNote.Octaves.Low);
            }
            else if (note.Equals(new LuteNote(LuteNote.Keys.Note8, LuteNote.Octaves.Low)) && _currentOctave == LuteNote.Octaves.Middle)
            {
                note = new LuteNote(LuteNote.Keys.Note1, LuteNote.Octaves.Middle);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case LuteNote.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToMiddle;
                    _currentOctave = LuteNote.Octaves.Middle;
                    break;
                case LuteNote.Octaves.Middle:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = LuteNote.Octaves.High;
                    break;
                case LuteNote.Octaves.High:
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
                case LuteNote.Octaves.Low:
                    break;
                case LuteNote.Octaves.Middle:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = LuteNote.Octaves.Low;
                    break;
                case LuteNote.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToMiddle;
                    _currentOctave = LuteNote.Octaves.Middle;
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
                case LuteNote.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case LuteNote.Octaves.Middle:
                    noteType = InstrumentSkillType.MiddleNote;
                    break;
                case LuteNote.Octaves.High:
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