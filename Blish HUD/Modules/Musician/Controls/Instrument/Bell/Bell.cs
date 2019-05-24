using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Bell : InstrumentType
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<BellNote.Keys, GuildWarsControls> NoteMap = new Dictionary<BellNote.Keys, GuildWarsControls>
        {
            {BellNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {BellNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {BellNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {BellNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {BellNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {BellNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {BellNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {BellNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private BellNote.Octaves _currentOctave = BellNote.Octaves.Low;

        public Bell(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var bellNote = BellNote.From(note);

            if (RequiresAction(bellNote))
            {
                if (bellNote.Key == BellNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    bellNote = OptimizeNote(bellNote);
                    PressNote(NoteMap[bellNote.Key]);
                }
            }
        }

        public override void GoToOctave(Note note)
        {
            var bellNote = BellNote.From(note);

            if (RequiresAction(bellNote))
            {
                bellNote = OptimizeNote(bellNote);

                while (_currentOctave != bellNote.Octave)
                {
                    if (_currentOctave < bellNote.Octave)
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

        private static bool RequiresAction(BellNote bellNote)
        {
            return bellNote.Key != BellNote.Keys.None;
        }

        private BellNote OptimizeNote(BellNote note)
        {
            if (note.Equals(new BellNote(BellNote.Keys.Note1, BellNote.Octaves.High)) && _currentOctave == BellNote.Octaves.Middle)
            {
                note = new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Middle);
            }
            else if (note.Equals(new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Middle)) && _currentOctave == BellNote.Octaves.High)
            {
                note = new BellNote(BellNote.Keys.Note1, BellNote.Octaves.High);
            }
            else if (note.Equals(new BellNote(BellNote.Keys.Note1, BellNote.Octaves.Middle)) && _currentOctave == BellNote.Octaves.Low)
            {
                note = new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Low);
            }
            else if (note.Equals(new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Low)) && _currentOctave == BellNote.Octaves.Middle)
            {
                note = new BellNote(BellNote.Keys.Note1, BellNote.Octaves.Middle);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case BellNote.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToMiddle;
                    _currentOctave = BellNote.Octaves.Middle;
                    break;
                case BellNote.Octaves.Middle:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = BellNote.Octaves.High;
                    break;
                case BellNote.Octaves.High:
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
                case BellNote.Octaves.Low:
                    break;
                case BellNote.Octaves.Middle:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = BellNote.Octaves.Low;
                    break;
                case BellNote.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToMiddle;
                    _currentOctave = BellNote.Octaves.Middle;
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
                case BellNote.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case BellNote.Octaves.Middle:
                    noteType = InstrumentSkillType.MiddleNote;
                    break;
                case BellNote.Octaves.High:
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