using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
using NAudio.Vorbis;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class Bell2SoundRepository
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{Bell2Note.Octaves.Low}", "C5"},
            {$"{GuildWarsControls.WeaponSkill2}{Bell2Note.Octaves.Low}", "D5"},
            {$"{GuildWarsControls.WeaponSkill3}{Bell2Note.Octaves.Low}", "E5"},
            {$"{GuildWarsControls.WeaponSkill4}{Bell2Note.Octaves.Low}", "F5"},
            {$"{GuildWarsControls.WeaponSkill5}{Bell2Note.Octaves.Low}", "G5"},
            {$"{GuildWarsControls.HealingSkill}{Bell2Note.Octaves.Low}", "A5"},
            {$"{GuildWarsControls.UtilitySkill1}{Bell2Note.Octaves.Low}", "B5"},
            {$"{GuildWarsControls.UtilitySkill2}{Bell2Note.Octaves.Low}", "C6"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{Bell2Note.Octaves.High}", "C6"},
            {$"{GuildWarsControls.WeaponSkill2}{Bell2Note.Octaves.High}", "D6"},
            {$"{GuildWarsControls.WeaponSkill3}{Bell2Note.Octaves.High}", "E6"},
            {$"{GuildWarsControls.WeaponSkill4}{Bell2Note.Octaves.High}", "F6"},
            {$"{GuildWarsControls.WeaponSkill5}{Bell2Note.Octaves.High}", "G6"},
            {$"{GuildWarsControls.HealingSkill}{Bell2Note.Octaves.High}", "A6"},
            {$"{GuildWarsControls.UtilitySkill1}{Bell2Note.Octaves.High}", "B6"},
            {$"{GuildWarsControls.UtilitySkill2}{Bell2Note.Octaves.High}", "C7"}
        };

        private static readonly Dictionary<string, CachedSound> Sound = new Dictionary<string, CachedSound>
        {
            {"C5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\C5.ogg"))))},
            {"D5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\D5.ogg"))))},
            {"E5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\E5.ogg"))))},
            {"F5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\F5.ogg"))))},
            {"G5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\G5.ogg"))))},
            {"A5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\A5.ogg"))))},
            {"B5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\B5.ogg"))))},
            {"C6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\C6.ogg"))))},
            {"D6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\D6.ogg"))))},
            {"E6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\E6.ogg"))))},
            {"F6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\F6.ogg"))))},
            {"G6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\G6.ogg"))))},
            {"A6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\A6.ogg"))))},
            {"B6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\B6.ogg"))))},
            {"C7", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bell2\C7.ogg"))))}

        };

        public CachedSound Get(string id)
        {
            return Sound[id];
        }

        public CachedSound Get(GuildWarsControls key, Bell2Note.Octaves octave)
        {
            return Sound[Map[$"{key}{octave}"]];
        }
    }
}