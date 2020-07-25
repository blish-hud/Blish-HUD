using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Blish_HUD.Debug;
using CSCore.CoreAudioAPI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameIntegration {
    public sealed class AudioIntegration : IUpdatable, IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<AudioIntegration>();

        private const int   CHECK_INTERVAL     = 250;
        private const int   AUDIOBUFFER_LENGTH = 20;
        private const float MAX_VOLUME         = 0.4f;

        private readonly GameIntegrationService _gameIntegration;
        private readonly MMDeviceEnumerator     _deviceEnumerator;
        private readonly RingBuffer<float>      _audioPeakBuffer = new RingBuffer<float>(AUDIOBUFFER_LENGTH);

        private AudioSessionManager2  _activeAudioDeviceManager;
        private AudioMeterInformation _processMeterInformation;

        private double _timeSinceCheck = 0;

        public float AverageGameVolume {
            get {
                float total = 0;
                for (int i = 0; i < _audioPeakBuffer.InternalBuffer.Length; i++) {
                    total += _audioPeakBuffer.InternalBuffer[i];
                }

                return MathHelper.Clamp(total / _audioPeakBuffer.InternalBuffer.Length, 0, MAX_VOLUME);
            }
        }

        public AudioIntegration(GameIntegrationService gameIntegration) {
            _gameIntegration  = gameIntegration;
            _deviceEnumerator = new MMDeviceEnumerator();

            PrepareListeners();
        }

        private void PrepareListeners() {
            UpdateActiveAudioDeviceManager();

            _deviceEnumerator.DefaultDeviceChanged += delegate { UpdateActiveAudioDeviceManager(); };
            _gameIntegration.Gw2Started            += delegate { UpdateProcessMeterInformation(); };
        }

        private void UpdateActiveAudioDeviceManager() {
            Task.Run(() => {
                // Must be called from an MTA thread.
                _activeAudioDeviceManager = GetActiveAudioDeviceManager();
            });

            if (_gameIntegration.Gw2IsRunning) {
                UpdateProcessMeterInformation();
            }
        }

        private void UpdateProcessMeterInformation() {
            foreach (var (process, meterInformation) in GetProcessMeters()) {
                if (process.Id == _gameIntegration.Gw2Process.Id) {
                    Logger.Debug("Found process associated audio session.");
                    _processMeterInformation = meterInformation;
                    break;
                }
            }

            _timeSinceCheck = 0;
        }

        public void Update(GameTime gameTime) {
            if (_processMeterInformation == null) return;

            _timeSinceCheck += gameTime.ElapsedGameTime.TotalMilliseconds;
            
            if (_timeSinceCheck > CHECK_INTERVAL) {
                _timeSinceCheck -= CHECK_INTERVAL;

                _audioPeakBuffer.PushValue(_processMeterInformation.GetPeakValue());
            }
        }

        /// <summary>
        /// Enumerates the available meters per process.
        /// The <see cref="Process"/> returned in the tuple is diposed after the enumeration completes.
        /// </summary>
        private IEnumerable<(Process Process, AudioMeterInformation MeterInformation)> GetProcessMeters() {
            using var sessionEnumerator = _activeAudioDeviceManager.GetSessionEnumerator();

            foreach (var session in sessionEnumerator) {
                using var processAudioSession = session.QueryInterface<AudioSessionControl2>();

                if (processAudioSession.Process == null) continue;

                var audioMeterInformation = session.QueryInterface<AudioMeterInformation>();

                yield return (processAudioSession.Process, audioMeterInformation);
            }

        }

        private AudioSessionManager2 GetActiveAudioDeviceManager() {
            using var device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            Logger.Debug($"Found default audio device: {device.FriendlyName}");

            return AudioSessionManager2.FromMMDevice(device);
        }

        public void Dispose() {
            _deviceEnumerator?.Dispose();
            _activeAudioDeviceManager?.Dispose();
            _processMeterInformation?.Dispose();
        }

    }
}
