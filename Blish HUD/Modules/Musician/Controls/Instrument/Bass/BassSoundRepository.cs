using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
using NAudio.Vorbis;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class BassSoundRepository
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.Low}", "C1"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.Low}", "D1"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.Low}", "E1"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.Low}", "F1"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.Low}", "G1"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.Low}", "A1"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.Low}", "B1"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.Low}", "C2"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{FluteNote.Octaves.High}", "C2"},
            {$"{GuildWarsControls.WeaponSkill2}{FluteNote.Octaves.High}", "D2"},
            {$"{GuildWarsControls.WeaponSkill3}{FluteNote.Octaves.High}", "E2"},
            {$"{GuildWarsControls.WeaponSkill4}{FluteNote.Octaves.High}", "F2"},
            {$"{GuildWarsControls.WeaponSkill5}{FluteNote.Octaves.High}", "G2"},
            {$"{GuildWarsControls.HealingSkill}{FluteNote.Octaves.High}", "A2"},
            {$"{GuildWarsControls.UtilitySkill1}{FluteNote.Octaves.High}", "B2"},
            {$"{GuildWarsControls.UtilitySkill2}{FluteNote.Octaves.High}", "C3"}
        };

        private static readonly Dictionary<string, CachedSound> Sound = new Dictionary<string, CachedSound>
        {
            {"C1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\C1.ogg"))))},
            {"D1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\D1.ogg"))))},
            {"E1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\E1.ogg"))))},
            {"F1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\F1.ogg"))))},
            {"G1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\G1.ogg"))))},
            {"A1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\A1.ogg"))))},
            {"B1", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\B1.ogg"))))},
            {"C2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\C2.ogg"))))},
            {"D2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\D2.ogg"))))},
            {"E2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\E2.ogg"))))},
            {"F2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\F2.ogg"))))},
            {"G2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\G2.ogg"))))},
            {"A2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\A2.ogg"))))},
            {"B2", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\B2.ogg"))))},
            {"C3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Bass\C3.ogg"))))}

        };

        public CachedSound Get(string id)
        {
            return Sound[id];
        }

        public CachedSound Get(GuildWarsControls key, BassNote.Octaves octave)
        {
            return Sound[Map[$"{key}{octave}"]];
        }
    }
}