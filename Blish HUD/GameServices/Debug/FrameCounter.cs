namespace Blish_HUD.Debug {
    public class FrameCounter {

        public double CurrentAverage { get; private set; }

        private readonly RingBuffer<double> _fpsSamples;

        public FrameCounter(int sampleCount) {
            _fpsSamples = new RingBuffer<double>(sampleCount);
        }

        public void Update(double deltaTime) {
            _fpsSamples.PushValue(1d / deltaTime);

            double total = 0;
            for (int i = 0; i < _fpsSamples.InternalBuffer.Length; i++) {
                total += _fpsSamples.InternalBuffer[i];
            }

            this.CurrentAverage = total / _fpsSamples.InternalBuffer.Length;
        }

    }
}
