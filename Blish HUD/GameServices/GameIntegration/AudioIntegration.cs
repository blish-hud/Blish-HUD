using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Debug;
using Blish_HUD.GameServices;
using Blish_HUD.Settings;
using CSCore.CoreAudioAPI;
using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.Xna.Framework;
using SharpDX.MediaFoundation;

namespace Blish_HUD.GameIntegration {
    public sealed class AudioIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<AudioIntegration>();

        private const int CHECK_INTERVAL = 250;
        private const int AUDIO_DEVICE_UPDATE_INTERVAL = 5000;
        private const int AUDIOBUFFER_LENGTH = 20;
        private const float MAX_VOLUME = 0.4f;
        private readonly SettingEntry<bool> _useGameAudio;
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private readonly RingBuffer<float> _audioPeakBuffer = new RingBuffer<float>(AUDIOBUFFER_LENGTH);
        private readonly SettingEntry<float> _volumeSetting;
        private List<(MMDevice AudioDevice, AudioMeterInformation MeterInformation)> _gw2AudioDevices = new List<(MMDevice AudioDevice, AudioMeterInformation MeterInformation)>();

        private double _timeSinceCheck = 0;
        private double _timeSinceAudioDeviceUpdate = 0;

        private float? _volume;

        /// <summary>
        /// This either provides  an estimated volume level for the application
        /// based on the volumes levels exhibited by the game
        /// or
        /// the set volume in settings.
        /// </summary>
        public float Volume => _volume ??= GetVolume();

        /// <summary>
        /// Current used AudioDevice. This either the same as GW2 is using
        /// or the selected one in the settings.
        /// </summary>
        public MMDevice AudioDevice { get; private set; }

        public AudioIntegration(GameIntegrationService service) : base(service) {
            var audioSettings = GameService.Settings.RegisterRootSettingCollection("OverlayConfiguration");
            _useGameAudio = audioSettings.DefineSetting("GameAudio", true, "Use Game Audio", "Let Blish HUD adjust depending on the ingame volume");
            _volumeSetting = audioSettings.DefineSetting("Volume", MAX_VOLUME/2, "Volume", "Volume");
            _volumeSetting.SetRange(0.0f, MAX_VOLUME);
            _deviceEnumerator = new MMDeviceEnumerator();

            PrepareListeners();
        }

        private void PrepareListeners() {
            UpdateActiveAudioDeviceManager();

            _deviceEnumerator.DefaultDeviceChanged += delegate { UpdateActiveAudioDeviceManager(); };
            _service.Gw2Started += delegate { UpdateActiveAudioDeviceManager(); };
        }

        private void UpdateActiveAudioDeviceManager() {
            Task.Run(() => {
                // Must be called from an MTA thread.
                InitializeProcessMeterInformations();
            });
        }

        public override void Update(GameTime gameTime) {
            if (_gw2AudioDevices.Count == 0) return;

            _timeSinceCheck += gameTime.ElapsedGameTime.TotalMilliseconds;
            _timeSinceAudioDeviceUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceCheck > CHECK_INTERVAL) {
                _timeSinceCheck -= CHECK_INTERVAL;

                try {
                    var peakValues = new List<((MMDevice AudioDevice, AudioMeterInformation MeterInformation) Device, float Peak)>();
                    foreach (var device in _gw2AudioDevices) {
                        peakValues.Add((device, device.MeterInformation.GetPeakValue()));
                    }

                    var (Device, Peak) = peakValues.OrderByDescending(x => x.Peak).First();
                    AudioDevice = Device.AudioDevice;
                    _audioPeakBuffer.PushValue(Peak);
                } catch (Exception e) {
                    // Punish audio clock for 10 seconds
                    _timeSinceCheck = -10000;

                    Logger.Debug(e, "Getting meter volume failed.");
                }

                _volume = null;
            }
            
            if (_timeSinceAudioDeviceUpdate > AUDIO_DEVICE_UPDATE_INTERVAL){
                _timeSinceAudioDeviceUpdate -= AUDIO_DEVICE_UPDATE_INTERVAL;
                // This is needed to react to sound device changes in gw2
                UpdateActiveAudioDeviceManager();
            }
        }

        private float GetVolume() {
            if (_useGameAudio.Value) {
                return CalculateAverageVolume();
            }

            return _volumeSetting.Value;
        }

        private float CalculateAverageVolume() {
            float total = 0;
            for (int i = 0; i < _audioPeakBuffer.InternalBuffer.Length; i++) {
                total += _audioPeakBuffer.InternalBuffer[i];
            }

            return MathHelper.Clamp(total / _audioPeakBuffer.InternalBuffer.Length, 0, MAX_VOLUME);
        }

        private void InitializeProcessMeterInformations() {
            if (!_service.Gw2IsRunning) return;

            _gw2AudioDevices.Clear();
            foreach (var device in _deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)) {
                using var sessionEnumerator = AudioSessionManager2.FromMMDevice(device).GetSessionEnumerator();

                bool shouldDispose = true;
                foreach (var session in sessionEnumerator) {
                    using var processAudioSession = session.QueryInterface<AudioSessionControl2>();

                    if (processAudioSession.Process.Id == _service.Gw2Process.Id) {
                        var audioMeterInformation = session.QueryInterface<AudioMeterInformation>();
                        _gw2AudioDevices.Add((device, session.QueryInterface<AudioMeterInformation>()));
                        shouldDispose = false;
                    }
                }

                if (shouldDispose) {
                    device.Dispose();
                }
            }
        }

        internal override void Unload() {
            _deviceEnumerator?.Dispose();

            foreach (var device in _gw2AudioDevices) {
                device.AudioDevice.Dispose();
                device.MeterInformation.Dispose();
            }
        }

    }
}
