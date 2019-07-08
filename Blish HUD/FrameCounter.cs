using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {
    public class FrameCounter {
        public FrameCounter() {
        }

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public const int MAXIMUM_SAMPLES = 100;

        private Queue<float> _sampleBuffer = new Queue<float>();

        public bool Update(float deltaTime) {
            this.CurrentFramesPerSecond = 1.0f / deltaTime;

            _sampleBuffer.Enqueue(this.CurrentFramesPerSecond);

            if (_sampleBuffer.Count > MAXIMUM_SAMPLES) {
                _sampleBuffer.Dequeue();
                this.AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            } else {
                this.AverageFramesPerSecond = this.CurrentFramesPerSecond;
            }

            this.TotalFrames++;
            this.TotalSeconds += deltaTime;
            return true;
        }
    }
}
