using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Debug {
    public class DebugCounter {

        #region Load Static

        private static readonly Stopwatch _sharedStopwatch;

        static DebugCounter() {
            _sharedStopwatch = Stopwatch.StartNew();
        }

        #endregion

        private readonly RingBuffer<long> _buffer;

        private long _intervalStartOffset;

        private float? _calculatedAverage = null;
        private long?  _calculatedTotal   = null;

        public DebugCounter(int bufferLength) {
            _buffer = new RingBuffer<long>(bufferLength);

            this.StartInterval();
        }

        public void StartInterval() {
            _intervalStartOffset = _sharedStopwatch.ElapsedMilliseconds;
        }

        public void EndInterval() {
            _buffer.PushValue(_sharedStopwatch.ElapsedMilliseconds - _intervalStartOffset);
            _calculatedAverage = null;
            _calculatedTotal   = null;
        }

        public float GetAverage() {
            return _calculatedAverage ?? (_calculatedAverage = (float)GetTotal() / _buffer.InternalBuffer.Length).Value;
        }

        public long GetTotal() {
            if (_calculatedTotal == null) {
                _calculatedTotal = 0;

                for (int i = 0; i < _buffer.InternalBuffer.Length; i++) {
                    _calculatedTotal += _buffer.InternalBuffer[i];
                }
            }

            return _calculatedTotal.Value;
        }

    }
}
