using System;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class FlutePreview : IKeyboard
    {
        private FluteNote.Octaves _octave = FluteNote.Octaves.Low;

        private readonly FluteSoundRepository _soundRepository = new FluteSoundRepository();

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
                    if (_octave == FluteNote.Octaves.Low)
                    {
                        IncreaseOctave();
                    }
                    else
                    {
                        DecreaseOctave();
                    }
                    break;
                case GuildWarsControls.EliteSkill:
                    AudioPlaybackEngine.Instance.StopSound();
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
                case FluteNote.Octaves.None:
                    break;
                case FluteNote.Octaves.Low:
                    _octave = FluteNote.Octaves.High;
                    break;
                case FluteNote.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DecreaseOctave()
        {
            switch (_octave)
            {
                case FluteNote.Octaves.None:
                    break;
                case FluteNote.Octaves.Low:
                    break;
                case FluteNote.Octaves.High:
                    _octave = FluteNote.Octaves.Low;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}