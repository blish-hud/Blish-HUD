
namespace Blish_HUD.Debug {
    public class FrameCounter {

        public float CurrentAverage { get; private set; }

        private readonly RingBuffer<float> _fpsSamples;

        public FrameCounter(int sampleCount) {
            _fpsSamples = new RingBuffer<float>(sampleCount);
        }

        public void Update(float deltaTime) {
            _fpsSamples.PushValue(1 / deltaTime);

            float total = 0;
            for (int i = 0; i < _fpsSamples.InternalBuffer.Length; i++) {
                total += _fpsSamples.InternalBuffer[i];
            }

            this.CurrentAverage = total / _fpsSamples.InternalBuffer.Length;
        }

    }
}
