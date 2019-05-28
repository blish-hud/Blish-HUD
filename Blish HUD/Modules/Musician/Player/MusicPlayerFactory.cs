using System;
using System.IO;
using Blish_HUD.Modules.Musician.Controls.Instrument;
using Blish_HUD.Modules.Musician.Notation.Parsers;
using Blish_HUD.Modules.Musician.Notation.Persistance;
using Blish_HUD.Modules.Musician.Player.Algorithms;
using Blish_HUD.Modules.Musician.Controls;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Player
{
    public enum KeyboardType
    {
        Emulated,
        Preview,
        Practice
    }
    internal static class MusicPlayerFactory
    {
        internal static MusicPlayer Create(RawMusicSheet rawMusicSheet, KeyboardType type)
        {
            return MusicBoxNotationMusicPlayerFactory(rawMusicSheet, type);
        }
        private static InstrumentType GetInstrument(string rawInstrument, KeyboardType type)
        {
            switch (rawInstrument)
            {
                case "harp":
                    return new Harp(type == KeyboardType.Preview ? new HarpPreview() : GetKeyboard(type));
                case "flute":
                    return new Flute(type == KeyboardType.Preview ? new FlutePreview() : GetKeyboard(type));
                case "lute":
                    return new Lute(type == KeyboardType.Preview ? new LutePreview() : GetKeyboard(type));
                case "horn":
                    return new Horn(type == KeyboardType.Preview ? new HornPreview() : GetKeyboard(type));
                case "bass":
                    return new Bass(type == KeyboardType.Preview ? new BassPreview() : GetKeyboard(type));
                case "bell":
                    return new Bell(type == KeyboardType.Preview ? new BellPreview() : GetKeyboard(type));
                case "bell2":
                    return new Bell2(type == KeyboardType.Preview ? new Bell2Preview() : GetKeyboard(type));
                default:
                    throw new NotSupportedException(rawInstrument);
            }
        }
        private static MusicPlayer MusicBoxNotationMusicPlayerFactory(RawMusicSheet rawMusicSheet, KeyboardType type)
        {
            var musicSheet = new MusicSheetParser(new ChordParser(new NoteParser(), rawMusicSheet.Instrument)).Parse(
                rawMusicSheet.Melody,
                int.Parse(rawMusicSheet.Tempo),
                int.Parse(rawMusicSheet.Meter.Split('/')[0]),
                int.Parse(rawMusicSheet.Meter.Split('/')[1]));


            var algorithm = rawMusicSheet.Algorithm == "favor notes"
                ? new FavorNotesAlgorithm() : (IPlayAlgorithm)new FavorChordsAlgorithm();

            return new MusicPlayer(
                musicSheet,
                GetInstrument(rawMusicSheet.Instrument, type),
                algorithm);
        }
        private static IKeyboard GetKeyboard(KeyboardType type)
        {
            switch (type)
            {
                case KeyboardType.Practice:
                    return (IKeyboard)new KeyboardPractice();
                case KeyboardType.Emulated:
                    return (IKeyboard)new Keyboard();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}