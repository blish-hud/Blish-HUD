using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Domain.Values;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Harp : Instrument
    {
        private static readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private static readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private static readonly Dictionary<HarpNote.Keys, GuildWarsControls> NoteMap = new Dictionary<HarpNote.Keys, GuildWarsControls>
        {
            {HarpNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {HarpNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {HarpNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {HarpNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {HarpNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {HarpNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {HarpNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {HarpNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private readonly IKeyboard _keyboard;

        private HarpNote.Octaves _currentOctave = HarpNote.Octaves.Middle;

        public Harp(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override void PlayNote(Note note)
        {
            var harpNote = HarpNote.From(note);

            if (RequiresAction(harpNote))
            {
                harpNote = OptimizeNote(harpNote);
                PressNote(NoteMap[harpNote.Key]);
            }
        }

        public override void GoToOctave(Note note)
        {
            var harpNote = HarpNote.From(note);

            if (RequiresAction(harpNote))
            {
                harpNote = OptimizeNote(harpNote);

                while (_currentOctave != harpNote.Octave)
                {
                    if (_currentOctave < harpNote.Octave)
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

        private static bool RequiresAction(HarpNote harpNote)
        {
            return harpNote.Key != HarpNote.Keys.None;
        }

        private HarpNote OptimizeNote(HarpNote note)
        {
            if (note.Equals(new HarpNote(HarpNote.Keys.Note1, HarpNote.Octaves.Middle)) && _currentOctave == HarpNote.Octaves.Low)
            {
                note = new HarpNote(HarpNote.Keys.Note8, HarpNote.Octaves.Low);
            }
            else if (note.Equals(new HarpNote(HarpNote.Keys.Note1, HarpNote.Octaves.High)) && _currentOctave == HarpNote.Octaves.Middle)
            {
                note = new HarpNote(HarpNote.Keys.Note8, HarpNote.Octaves.Middle);
            }
            return note;
        }

        private void IncreaseOctave()
        {
            var noteType = InstrumentSkillType.IncreaseOctaveToHigh;
            switch (_currentOctave)
            {
                case HarpNote.Octaves.Low:
                    noteType = InstrumentSkillType.IncreaseOctaveToMiddle;
                    _currentOctave = HarpNote.Octaves.Middle;
                    break;
                case HarpNote.Octaves.Middle:
                    noteType = InstrumentSkillType.IncreaseOctaveToHigh;
                    _currentOctave = HarpNote.Octaves.High;
                    break;
                case HarpNote.Octaves.High:
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
                case HarpNote.Octaves.Low:
                    break;
                case HarpNote.Octaves.Middle:
                    noteType = InstrumentSkillType.DecreaseOctaveToLow;
                    _currentOctave = HarpNote.Octaves.Low;
                    break;
                case HarpNote.Octaves.High:
                    noteType = InstrumentSkillType.DecreaseOctaveToMiddle;
                    _currentOctave = HarpNote.Octaves.Middle;
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
                case HarpNote.Octaves.Low:
                    noteType = InstrumentSkillType.LowNote;
                    break;
                case HarpNote.Octaves.Middle:
                    noteType = InstrumentSkillType.MiddleNote;
                    break;
                case HarpNote.Octaves.High:
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