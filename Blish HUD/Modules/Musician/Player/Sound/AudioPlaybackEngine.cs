using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Blish_HUD.Modules.Musician.Player.Sound
{
    public class AudioPlaybackEngine : IDisposable
    {
        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine();

        private readonly MixingSampleProvider _mixer;
        private WaveOutEvent _outputDevice;

        public AudioPlaybackEngine()
        {
            _outputDevice = new WaveOutEvent();
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true
            };

            _outputDevice.Init(new SampleToWaveProvider(new VolumeSampleProvider(_mixer){ Volume = 0.2f }));
            _outputDevice.Play();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            _mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }
        public void StopSound() {
            _mixer.RemoveAllMixerInputs();
        }
        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_outputDevice != null)
                {
                    _outputDevice.Dispose();
                    _outputDevice = null;
                }
            }
        }
    }
}