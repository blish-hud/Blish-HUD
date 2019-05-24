using System.Diagnostics;
using System.Linq;
using System.Threading;
using System;
using Blish_HUD.Modules.Musician.Domain.Values;
using Blish_HUD.Modules.Musician.Controls.Instrument;
namespace Blish_HUD.Modules.Musician.Player.Algorithms
{
    public class FavorNotesAlgorithm : IPlayAlgorithm
    {
        public void Play(InstrumentType instrument, MetronomeMark metronomeMark, ChordOffset[] melody)
        {
            PrepareChordsOctave(instrument, melody[0].Chord);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var strumIndex = 0; strumIndex < melody.Length;)
            {
                var strum = melody[strumIndex];

                if (stopwatch.ElapsedMilliseconds > metronomeMark.WholeNoteLength.Multiply(strum.Offest).TotalMilliseconds)
                {
                    var chord = strum.Chord;

                    PlayChord(instrument, chord);

                    if (strumIndex < melody.Length - 1)
                    {
                        PrepareChordsOctave(instrument, melody[strumIndex + 1].Chord);
                    }

                    strumIndex++;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }
            }

            stopwatch.Stop();
        }

        private static void PrepareChordsOctave(InstrumentType instrument, Chord chord)
        {
            instrument.GoToOctave(chord.Notes.First());
        }

        private static void PlayChord(InstrumentType instrument, Chord chord)
        {
            var notes = chord.Notes.ToArray();

            for (var noteIndex = 0; noteIndex < notes.Length; noteIndex++)
            {
                instrument.PlayNote(notes[noteIndex]);

                if (noteIndex < notes.Length - 1)
                {
                    PrepareNoteOctave(instrument, notes[noteIndex + 1]);
                }
            }
        }

        private static void PrepareNoteOctave(InstrumentType instrument, Note note)
        {
            instrument.GoToOctave(note);
        }
    }
}