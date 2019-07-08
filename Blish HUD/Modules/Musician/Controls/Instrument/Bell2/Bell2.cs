using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Bell2 : Instrument
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<Bell2Note.Keys, GuildWarsControls> NoteMap = new Dictionary<Bell2Note.Keys, GuildWarsControls>
        {
            {Bell2Note.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {Bell2Note.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {Bell2Note.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {Bell2Note.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {Bell2Note.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {Bell2Note.Keys.Note6, GuildWarsControls.HealingSkill},
            {Bell2Note.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {Bell2Note.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private Bell2Note.Octaves _currentOctave = Bell2Note.Octaves.Low;

        public Bell2(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var bell2Note = Bell2Note.From(note);

            if (RequiresAction(bell2Note))
            {
                if (bell2Note.Key == Bell2Note.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    bell2Note = OptimizeNote(bell2Note);
                    PressNote(NoteMap[bell2Note.Key]);
                }
            }
        }

        public override void GoToOctave(Note note)
        {
            var bell2Note = Bell2Note.From(note);

            if (RequiresAction(bell2Note))
            {
                bell2Note = OptimizeNote(bell2Note);

                while (_currentOctave != bell2Note.Octave)
                {
                    if (_currentOctave < bell2Note.Octave)
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

        private static bool RequiresAction(Bell2Note bell2Note)
        {
            return bell2Note.Key != Bell2Note.Keys.None;
        }

        private Bell2Note OptimizeNote(Bell2Note note)
        {
            if (note.Equals(new Bell2Note(Bell2Note.Keys.Note1, Bell2Note.Octaves.High)) && _currentOctave == Bell2Note.Octaves.Low)
            {
                note = new Bell2Note(Bell2Note.Keys.Note8, Bell2Note.Octaves.Low);
            }
            else if (note.Equals(new Bell2Note(Bell2Note.Keys.Note8, Bell2Note.Octaves.Low)) && _currentOctave == Bell2Note.Octaves.High)
            {
                note = new Bell2Note(Bell2Note.Keys.Note1, Bell2Note.Octaves.High);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case Bell2Note.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = Bell2Note.Octaves.High;
                    break;
                case Bell2Note.Octaves.High:
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
                case Bell2Note.Octaves.Low:
                    break;
                case Bell2Note.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = Bell2Note.Octaves.Low;
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
                case Bell2Note.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case Bell2Note.Octaves.High:
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