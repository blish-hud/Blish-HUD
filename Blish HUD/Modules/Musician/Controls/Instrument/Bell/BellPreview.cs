using System;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class BellPreview : IKeyboard
    {
        private BellNote.Octaves _octave = BellNote.Octaves.Middle;

        private readonly BellSoundRepository _soundRepository = new BellSoundRepository();

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

        public void Release(GuildWarsControls key){}

        private void IncreaseOctave()
        {
            switch (_octave)
            {
                case BellNote.Octaves.None:
                    break;
                case BellNote.Octaves.Low:
                    _octave = BellNote.Octaves.Middle;
                    break;
                case BellNote.Octaves.Middle:
                    _octave = BellNote.Octaves.High;
                    break;
                case BellNote.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DecreaseOctave()
        {
            switch (_octave)
            {
                case BellNote.Octaves.None:
                    break;
                case BellNote.Octaves.Low:
                    break;
                case BellNote.Octaves.Middle:
                    _octave = BellNote.Octaves.Low;
                    break;
                case BellNote.Octaves.High:
                    _octave = BellNote.Octaves.Middle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}