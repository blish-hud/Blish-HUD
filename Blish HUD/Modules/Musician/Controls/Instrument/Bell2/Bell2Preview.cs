using System;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Bell2Preview : IKeyboard
    {
        private Bell2Note.Octaves _octave = Bell2Note.Octaves.Low;

        private readonly Bell2SoundRepository _soundRepository = new Bell2SoundRepository();

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
                case Bell2Note.Octaves.None:
                    break;
                case Bell2Note.Octaves.Low:
                    _octave = Bell2Note.Octaves.High;
                    break;
                case Bell2Note.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DecreaseOctave()
        {
            switch (_octave)
            {
                case Bell2Note.Octaves.None:
                    break;
                case Bell2Note.Octaves.Low:
                    break;
                case Bell2Note.Octaves.High:
                    _octave = Bell2Note.Octaves.Low;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}