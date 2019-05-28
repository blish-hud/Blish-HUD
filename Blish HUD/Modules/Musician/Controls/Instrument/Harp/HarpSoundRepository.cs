using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Modules.Musician.Player.Sound;
using NAudio.Vorbis;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class HarpSoundRepository
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            {$"{GuildWarsControls.WeaponSkill1}{HarpNote.Octaves.Low}", "C3"},
            {$"{GuildWarsControls.WeaponSkill2}{HarpNote.Octaves.Low}", "D3"},
            {$"{GuildWarsControls.WeaponSkill3}{HarpNote.Octaves.Low}", "E3"},
            {$"{GuildWarsControls.WeaponSkill4}{HarpNote.Octaves.Low}", "F3"},
            {$"{GuildWarsControls.WeaponSkill5}{HarpNote.Octaves.Low}", "G3"},
            {$"{GuildWarsControls.HealingSkill}{HarpNote.Octaves.Low}", "A3"},
            {$"{GuildWarsControls.UtilitySkill1}{HarpNote.Octaves.Low}", "B3"},
            {$"{GuildWarsControls.UtilitySkill2}{HarpNote.Octaves.Low}", "C4"},
            {$"{GuildWarsControls.WeaponSkill1}{HarpNote.Octaves.Middle}", "C4"},
            {$"{GuildWarsControls.WeaponSkill2}{HarpNote.Octaves.Middle}", "D4"},
            {$"{GuildWarsControls.WeaponSkill3}{HarpNote.Octaves.Middle}", "E4"},
            {$"{GuildWarsControls.WeaponSkill4}{HarpNote.Octaves.Middle}", "F4"},
            {$"{GuildWarsControls.WeaponSkill5}{HarpNote.Octaves.Middle}", "G4"},
            {$"{GuildWarsControls.HealingSkill}{HarpNote.Octaves.Middle}", "A4"},
            {$"{GuildWarsControls.UtilitySkill1}{HarpNote.Octaves.Middle}", "B4"},
            {$"{GuildWarsControls.UtilitySkill2}{HarpNote.Octaves.Middle}", "C5"},
            {$"{GuildWarsControls.WeaponSkill1}{HarpNote.Octaves.High}", "C5"},
            {$"{GuildWarsControls.WeaponSkill2}{HarpNote.Octaves.High}", "D5"},
            {$"{GuildWarsControls.WeaponSkill3}{HarpNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill4}{HarpNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill5}{HarpNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.HealingSkill}{HarpNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.UtilitySkill1}{HarpNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.UtilitySkill2}{HarpNote.Octaves.High}", "C6"}
        };

        private static readonly Dictionary<string, CachedSound> Sound = new Dictionary<string, CachedSound>
        {
            {"C3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\C3.ogg"))))},
            {"D3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\D3.ogg"))))},
            {"E3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\E3.ogg"))))},
            {"F3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\F3.ogg"))))},
            {"G3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\G3.ogg"))))},
            {"A3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\A3.ogg"))))},
            {"B3", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\B3.ogg"))))},
            {"C4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\C4.ogg"))))},
            {"D4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\D4.ogg"))))},
            {"E4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\E4.ogg"))))},
            {"F4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\F4.ogg"))))},
            {"G4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\G4.ogg"))))},
            {"A4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\A4.ogg"))))},
            {"B4", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\B4.ogg"))))},
            {"C5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\C5.ogg"))))},
            {"D5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\D5.ogg"))))},
            {"E5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\E5.ogg"))))},
            {"F5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\F5.ogg"))))},
            {"G5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\G5.ogg"))))},
            {"A5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\A5.ogg"))))},
            {"B5", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\B5.ogg"))))},
            {"C6", new CachedSound(new AutoDisposeFileReader(new VorbisWaveReader(GameService.Content.GetFile(@"instruments\Harp\C6.ogg"))))}
        };

        public CachedSound Get(string id)
        {
            return Sound[id];
        }

        public CachedSound Get(GuildWarsControls key, HarpNote.Octaves octave)
        {
            return Sound[Map[$"{key}{octave}"]];
        }
    }
}