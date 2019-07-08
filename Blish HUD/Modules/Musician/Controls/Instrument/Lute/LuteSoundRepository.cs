using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
using NAudio.Vorbis;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class LuteSoundRepository
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{LuteNote.Octaves.Low}", "C3"},
            {$"{GuildWarsControls.WeaponSkill2}{LuteNote.Octaves.Low}", "D3"},
            {$"{GuildWarsControls.WeaponSkill3}{LuteNote.Octaves.Low}", "E3"},
            {$"{GuildWarsControls.WeaponSkill4}{LuteNote.Octaves.Low}", "F3"},
            {$"{GuildWarsControls.WeaponSkill5}{LuteNote.Octaves.Low}", "G3"},
            {$"{GuildWarsControls.HealingSkill}{LuteNote.Octaves.Low}", "A3"},
            {$"{GuildWarsControls.UtilitySkill1}{LuteNote.Octaves.Low}", "B3"},
            {$"{GuildWarsControls.UtilitySkill2}{LuteNote.Octaves.Low}", "C4"},
            // Middle Octave
            {$"{GuildWarsControls.WeaponSkill1}{LuteNote.Octaves.Middle}", "C4"},
            {$"{GuildWarsControls.WeaponSkill2}{LuteNote.Octaves.Middle}", "D4"},
            {$"{GuildWarsControls.WeaponSkill3}{LuteNote.Octaves.Middle}", "E4"},
            {$"{GuildWarsControls.WeaponSkill4}{LuteNote.Octaves.Middle}", "F4"},
            {$"{GuildWarsControls.WeaponSkill5}{LuteNote.Octaves.Middle}", "G4"},
            {$"{GuildWarsControls.HealingSkill}{LuteNote.Octaves.Middle}", "A4"},
            {$"{GuildWarsControls.UtilitySkill1}{LuteNote.Octaves.Middle}", "B4"},
            {$"{GuildWarsControls.UtilitySkill2}{LuteNote.Octaves.Middle}", "C5"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{LuteNote.Octaves.High}", "C5"},
            {$"{GuildWarsControls.WeaponSkill2}{LuteNote.Octaves.High}", "D5"},
            {$"{GuildWarsControls.WeaponSkill3}{LuteNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill4}{LuteNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill5}{LuteNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.HealingSkill}{LuteNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.UtilitySkill1}{LuteNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.UtilitySkill2}{LuteNote.Octaves.High}", "C6"},
        };

        private static readonly Dictionary<string, CachedSound> Sound = new Dictionary<string, CachedSound>
        {
            {"C3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\C3.ogg"))))},
            {"D3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\D3.ogg"))))},
            {"E3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\E3.ogg"))))},
            {"F3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\F3.ogg"))))},
            {"G3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\G3.ogg"))))},
            {"A3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\A3.ogg"))))},
            {"B3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\B3.ogg"))))},
            {"C4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\C4.ogg"))))},
            {"D4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\D4.ogg"))))},
            {"E4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\E4.ogg"))))},
            {"F4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\F4.ogg"))))},
            {"G4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\G4.ogg"))))},
            {"A4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\A4.ogg"))))},
            {"B4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\B4.ogg"))))},
            {"C5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\C5.ogg"))))},
            {"D5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\D5.ogg"))))},
            {"E5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\E5.ogg"))))},
            {"F5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\F5.ogg"))))},
            {"G5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\G5.ogg"))))},
            {"A5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\A5.ogg"))))},
            {"B5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\B5.ogg"))))},
            {"C6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Lute\C6.ogg"))))}

        };

        public CachedSound Get(string id)
        {
            return Sound[id];
        }

        public CachedSound Get(GuildWarsControls key, LuteNote.Octaves octave)
        {
            return Sound[Map[$"{key}{octave}"]];
        }
    }
}