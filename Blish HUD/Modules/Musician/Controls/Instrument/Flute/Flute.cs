using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Flute : InstrumentType
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<FluteNote.Keys, GuildWarsControls> NoteMap = new Dictionary<FluteNote.Keys, GuildWarsControls>
        {
            {FluteNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {FluteNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {FluteNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {FluteNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {FluteNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {FluteNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {FluteNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {FluteNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private FluteNote.Octaves _currentOctave = FluteNote.Octaves.Low;

        public Flute(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var fluteNote = FluteNote.From(note);

            if (RequiresAction(fluteNote))
            {
                if (fluteNote.Key == FluteNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    fluteNote = OptimizeNote(fluteNote);
                    PressNote(NoteMap[fluteNote.Key]);
                }
            }
        }

        public override void GoToOctave(Note note)
        {
            var fluteNote = FluteNote.From(note);

            if (RequiresAction(fluteNote))
            {
                fluteNote = OptimizeNote(fluteNote);

                while (_currentOctave != fluteNote.Octave)
                {
                    if (_currentOctave < fluteNote.Octave)
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

        private static bool RequiresAction(FluteNote fluteNote)
        {
            return fluteNote.Key != FluteNote.Keys.None;
        }

        private FluteNote OptimizeNote(FluteNote note)
        {
            if (note.Equals(new FluteNote(FluteNote.Keys.Note1, FluteNote.Octaves.High)) && _currentOctave == FluteNote.Octaves.Low)
            {
                note = new FluteNote(FluteNote.Keys.Note8, FluteNote.Octaves.Low);
            }
            else if (note.Equals(new FluteNote(FluteNote.Keys.Note8, FluteNote.Octaves.Low)) && _currentOctave == FluteNote.Octaves.High)
            {
                note = new FluteNote(FluteNote.Keys.Note1, FluteNote.Octaves.High);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case FluteNote.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = FluteNote.Octaves.High;
                    break;
                case FluteNote.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _keyboard.Press(GuildWarsControls.UtilitySkill3);
            _keyboard.Release(GuildWarsControls.UtilitySkill3);

            Thread.Sleep(OctaveTimeout);
        }

        private void DecreaseOctave()
        {
            var noteType = InstrumentSkillType.DecreaseOctaveToLow;
            switch (_currentOctave)
            {
                case FluteNote.Octaves.Low:
                    break;
                case FluteNote.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = FluteNote.Octaves.Low;
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
                case FluteNote.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case FluteNote.Octaves.High:
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