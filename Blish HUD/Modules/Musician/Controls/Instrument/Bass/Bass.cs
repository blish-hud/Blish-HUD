using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Bass : InstrumentType
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<BassNote.Keys, GuildWarsControls> NoteMap = new Dictionary<BassNote.Keys, GuildWarsControls>
        {
            {BassNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {BassNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {BassNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {BassNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {BassNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {BassNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {BassNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {BassNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private BassNote.Octaves _currentOctave = BassNote.Octaves.Low;

        public Bass(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var bassNote = BassNote.From(note);

            if (RequiresAction(bassNote))
            {
                if (bassNote.Key == BassNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    bassNote = OptimizeNote(bassNote);
                    PressNote(NoteMap[bassNote.Key]);
                }
            }
        }

        public override void GoToOctave(Note note)
        {
            var bassNote = BassNote.From(note);

            if (RequiresAction(bassNote))
            {
                bassNote = OptimizeNote(bassNote);

                while (_currentOctave != bassNote.Octave)
                {
                    if (_currentOctave < bassNote.Octave)
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

        private static bool RequiresAction(BassNote bassNote)
        {
            return bassNote.Key != BassNote.Keys.None;
        }

        private BassNote OptimizeNote(BassNote note)
        {
            if (note.Equals(new BassNote(BassNote.Keys.Note1, BassNote.Octaves.High)) && _currentOctave == BassNote.Octaves.Low)
            {
                note = new BassNote(BassNote.Keys.Note8, BassNote.Octaves.Low);
            }
            else if (note.Equals(new BassNote(BassNote.Keys.Note8, BassNote.Octaves.Low)) && _currentOctave == BassNote.Octaves.High)
            {
                note = new BassNote(BassNote.Keys.Note1, BassNote.Octaves.High);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case BassNote.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = BassNote.Octaves.High;
                    break;
                case BassNote.Octaves.High:
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
                case BassNote.Octaves.Low:
                    break;
                case BassNote.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = BassNote.Octaves.Low;
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
            var noteType = InstrumentSkillType.LowNote;
            switch (_currentOctave)
            {
                case BassNote.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case BassNote.Octaves.High:
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