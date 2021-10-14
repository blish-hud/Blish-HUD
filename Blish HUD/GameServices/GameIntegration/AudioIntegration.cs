using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Blish_HUD.Debug;
using Blish_HUD.GameServices;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using NAudio.CoreAudioApi;

namespace Blish_HUD.GameIntegration {
    public sealed class AudioIntegration : ServiceModule<GameIntegrationService> {
        private static readonly Logger Logger = Logger.GetLogger<AudioIntegration>();

        public enum Devices {
            [Description("GW2 Output Device")]
            Gw2OutputDevice,
            DefaultDevice
        }

        private const    string                APPLICATION_SETTINGS         = "OverlayConfiguration";
        private const    string                USEGAMEVOLUME_SETTINGS       = "GameVolume";
        private const    string                VOLUME_SETTINGS              = "Volume";
        private const    string                DEVICE_SETTINGS              = "OutputDevice";
        private const    int                   CHECK_INTERVAL               = 250;
        private const    int                   AUDIO_DEVICE_UPDATE_INTERVAL = 10000;
        private const    int                   AUDIOBUFFER_LENGTH           = 20;
        private const    float                 MAX_VOLUME                   = 0.4f;
        private readonly RingBuffer<float>     _audioPeakBuffer             = new RingBuffer<float>(AUDIOBUFFER_LENGTH);
        private readonly MMDeviceEnumerator    _deviceEnumerator;
        private          SettingEntry<bool>    _useGameVolume;
        private          SettingEntry<Devices> _deviceSetting;
        private          SettingEntry<float>   _volumeSetting;

        private readonly AudioEndpointNotificationReceiver _audioEndpointNotificationReceiver;
        private readonly List<(MMDevice AudioDevice, AudioMeterInformation MeterInformation)> _gw2AudioDevices = new List<(MMDevice AudioDevice, AudioMeterInformation MeterInformation)>();

        private double _timeSinceCheck             = 0;
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
            _audioEndpointNotificationReceiver = new AudioEndpointNotificationReceiver();
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        public override void Load() {
            var audioSettings = GameService.Settings.RegisterRootSettingCollection(APPLICATION_SETTINGS);
            _useGameVolume = audioSettings.DefineSetting(USEGAMEVOLUME_SETTINGS, true, () => Strings.GameServices.OverlayService.Setting_UseGameVolume_DisplayName, () => Strings.GameServices.OverlayService.Setting_UseGameVolume_Description);
            _volumeSetting = audioSettings.DefineSetting(VOLUME_SETTINGS, MAX_VOLUME / 2, () => Strings.GameServices.OverlayService.Setting_Volume_DisplayName, () => Strings.GameServices.OverlayService.Setting_Volume_Description);
            _volumeSetting.SetRange(0.0f, MAX_VOLUME);

            _deviceSetting = audioSettings.DefineSetting(DEVICE_SETTINGS, Devices.Gw2OutputDevice, () => Strings.GameServices.OverlayService.Setting_AudioDevice_DisplayName, () => Strings.GameServices.OverlayService.Setting_AudioDevice_Description + " (This setting is temporarily disabled in this version)");
            // This setting is disabled (so we force it to show "default")
            // See https://github.com/blish-hud/Blish-HUD/issues/355#issuecomment-787713586
            _deviceSetting.Value = Devices.DefaultDevice;
            _deviceSetting.SetDisabled();

            PrepareListeners();
            UpdateAudioDevice();
        }

        private void PrepareListeners() {
            _deviceEnumerator.RegisterEndpointNotificationCallback(_audioEndpointNotificationReceiver);

            _audioEndpointNotificationReceiver.DefaultDeviceChanged += delegate { UpdateAudioDevice(); };
            _deviceSetting.SettingChanged                           += delegate { UpdateAudioDevice(); };
            _service.Gw2Instance.Gw2Started                         += delegate { InitializeProcessMeterInformations(); };
        }

        public override void Update(GameTime gameTime) {
            if (_gw2AudioDevices.Count == 0 || !_service.Gw2Instance.Gw2IsRunning) return;

            _timeSinceCheck += gameTime.ElapsedGameTime.TotalMilliseconds;
            _timeSinceAudioDeviceUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceCheck > CHECK_INTERVAL) {
                _timeSinceCheck -= CHECK_INTERVAL;

                try {
                    var peakValues = new List<((MMDevice AudioDevice, AudioMeterInformation MeterInformation) Device, float Peak)>();
                    foreach (var device in _gw2AudioDevices) {
                        peakValues.Add((device, device.MeterInformation.MasterPeakValue));
                    }

                    var (Device, Peak) = peakValues.OrderByDescending(x => x.Peak).First();

                    if (_deviceSetting.Value == Devices.Gw2OutputDevice) {
                        AudioDevice = Device.AudioDevice;
                    }

                    _audioPeakBuffer.PushValue(Peak);
                } catch (Exception e) {
                    // Punish audio clock for 10 seconds
                    _timeSinceCheck = -10000;

                    Logger.Debug(e, "Getting meter volume failed.");
                }

                _volume = null;
            }

            if (_timeSinceAudioDeviceUpdate > AUDIO_DEVICE_UPDATE_INTERVAL) {
                _timeSinceAudioDeviceUpdate -= AUDIO_DEVICE_UPDATE_INTERVAL;
                // This is needed to react to sound device changes in gw2
                InitializeProcessMeterInformations();
            }
        }

        private float GetVolume() {
            if (_useGameVolume.Value) {
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

        private void UpdateAudioDevice() {
            if (_deviceSetting.Value == Devices.DefaultDevice) {
                if (TryGetDefaultAudioEndpoint(_deviceEnumerator, DataFlow.Render, Role.Multimedia, out MMDevice defaultDevice)) {
                    this.AudioDevice = defaultDevice;
                } else {
                    this.AudioDevice = null;
                }
            }

            InitializeProcessMeterInformations();
        }

        private static bool TryGetDefaultAudioEndpoint(MMDeviceEnumerator deviceEnumerator, DataFlow dataFlow, Role role, out MMDevice device) {
            try {
                device = deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role);
                return true;
            } catch (COMException ex) when ((uint)ex.HResult == 0x80070490) {
                // HResult 0x80070490 = Element not found
                device = null;
                return false;
            }
        }

        private void InitializeProcessMeterInformations() {
            if (!_service.Gw2Instance.Gw2IsRunning) return;

            _gw2AudioDevices.Clear();
            foreach (var device in _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)) {
                var sessionEnumerator = device.AudioSessionManager.Sessions;

                bool shouldDispose = true;
                for (int i = 0; i < sessionEnumerator.Count; i++) {
                    using var audioSession = sessionEnumerator[i];

                    if (audioSession.GetProcessID == _service.Gw2Instance.Gw2Process.Id) {
                        _gw2AudioDevices.Add((device, audioSession.AudioMeterInformation));
                        shouldDispose = false;
                    }
                }

                if (shouldDispose) {
                    device.Dispose();
                }
            }
        }

        public override void Unload() {
            _deviceEnumerator.UnregisterEndpointNotificationCallback(_audioEndpointNotificationReceiver);
            _deviceEnumerator.Dispose();

            foreach (var device in _gw2AudioDevices) {
                device.AudioDevice.Dispose();
            }
        }

    }
}
