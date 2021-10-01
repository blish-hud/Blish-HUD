using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace Blish_HUD.GameIntegration {
    internal class AudioEndpointNotificationReceiver : IMMNotificationClient {

        public delegate void DefaultDeviceChangedDelegate(DataFlow flow, Role role, string defaultDeviceID);
        public delegate void DeviceAddRemoveDelegate(string deviceId);
        public delegate void DeviceStateChangeDelegate(string deviceId, DeviceState newState);
        public delegate void DevicePropertyValueChangedDelegate(string deviceId, PropertyKey key);

        public event DefaultDeviceChangedDelegate DefaultDeviceChanged;
        public event DeviceAddRemoveDelegate DeviceAdded;
        public event DeviceAddRemoveDelegate DeviceRemoved;
        public event DeviceStateChangeDelegate DeviceStateChanged;
        public event DevicePropertyValueChangedDelegate PropertyValueChanged;

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) {
            DefaultDeviceChanged?.Invoke(flow, role, defaultDeviceId);
        }

        public void OnDeviceAdded(string deviceId) {
            DeviceAdded?.Invoke(deviceId);
        }

        public void OnDeviceRemoved(string deviceId) {
            DeviceRemoved?.Invoke(deviceId);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState) {
            DeviceStateChanged?.Invoke(deviceId, newState);
        }

        public void OnPropertyValueChanged(string deviceId, PropertyKey key) {
            PropertyValueChanged?.Invoke(deviceId, key);
        }
    }
}
