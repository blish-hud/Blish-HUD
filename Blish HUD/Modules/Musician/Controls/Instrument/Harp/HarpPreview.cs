using System;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class HarpPreview : IKeyboard
    {
        private HarpNote.Octaves _octave = HarpNote.Octaves.Middle;

        private readonly HarpSoundRepository _soundRepository = new HarpSoundRepository();

        public void Press(GuildWarsControls key)
        {
            switch (key)
            {
                case GuildWarsControls.WeaponSkill1:
                case GuildWarsControls.WeaponSkill2:
                case GuildWarsControls.WeaponSkill3:
                case GuildWarsControls.WeaponSkill4:
                case GuildWarsControls.WeaponSkill5:
                case GuildWarsControls.HealingSkill:
                case GuildWarsControls.UtilitySkill1:
                case GuildWarsControls.UtilitySkill2:
                    AudioPlaybackEngine.Instance.PlaySound(_soundRepository.Get(key, _octave));
                    break;
                case GuildWarsControls.UtilitySkill3:
                    DecreaseOctave();
                    break;
                case GuildWarsControls.EliteSkill:
                    IncreaseOctave();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Release(GuildWarsControls key)
        {
        }

        private void IncreaseOctave()
        {
            switch (_octave)
            {
                case HarpNote.Octaves.None:
                    break;
                case HarpNote.Octaves.Low:
                    _octave = HarpNote.Octaves.Middle;
                    break;
                case HarpNote.Octaves.Middle:
                    _octave = HarpNote.Octaves.High;
                    break;
                case HarpNote.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DecreaseOctave()
        {
            switch (_octave)
            {
                case HarpNote.Octaves.None:
                    break;
                case HarpNote.Octaves.Low:
                    break;
                case HarpNote.Octaves.Middle:
                    _octave = HarpNote.Octaves.Low;
                    break;
                case HarpNote.Octaves.High:
                    _octave = HarpNote.Octaves.Middle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}