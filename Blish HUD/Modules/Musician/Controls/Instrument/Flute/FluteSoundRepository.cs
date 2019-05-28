using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
using NAudio.Vorbis;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class FluteSoundRepository
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.Low}", "E4"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.Low}", "F4"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.Low}", "G4"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.Low}", "A4"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.Low}", "B4"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.Low}", "C5"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.Low}", "D5"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.Low}", "E5"},
            //{$"{GuildWarsControls.UtilitySkill3}{FluteNote.Octaves.Low}", "Increase Octave"},
            //{$"{GuildWarsControls.EliteSkill}{FluteNote.Octaves.Low}", "Stop Playing"},

            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.High}", "C6"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.High}", "D6"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.High}", "E6"}
            //{$"{GuildWarsControls.UtilitySkill3}{FluteNote.Octaves.Low}", "Decrease Octave"},
            //{$"{GuildWarsControls.EliteSkill}{FluteNote.Octaves.Low}", "Stop Playing"},
        };

        private static readonly Dictionary<string, CachedSound> Sound = new Dictionary<string, CachedSound>
        {
            {"E4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\E4.ogg"))))},
            {"F4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\F4.ogg"))))},
            {"G4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\G4.ogg"))))},
            {"A4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\A4.ogg"))))},
            {"B4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\B4.ogg"))))},
            {"C5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\C5.ogg"))))},
            {"D5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\D5.ogg"))))},
            {"E5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\E5.ogg"))))},
            {"F5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\F5.ogg"))))},
            {"G5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\G5.ogg"))))},
            {"A5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\A5.ogg"))))},
            {"B5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\B5.ogg"))))},
            {"C6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\C6.ogg"))))},
            {"D6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\D6.ogg"))))},
            {"E6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Flute\E6.ogg"))))}

        };

        public CachedSound Get(string id)
        {
            return Sound[id];
        }

        public CachedSound Get(GuildWarsControls key, FluteNote.Octaves octave)
        {
            return Sound[Map[$"{key}{octave}"]];
        }
    }
}