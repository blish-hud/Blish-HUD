namespace Blish_HUD.Debug {
    /// <summary>
    /// [NOT THREAD-SAFE] A fixed capacity buffer which overwrites itself as the index wraps.
    /// </summary>
    /// <typeparam name="T">The <c>Type</c> the <see cref="RingBuffer{T}"/> contains.</typeparam>
    public class RingBuffer<T> {

        private int _ringIndex;

        /// <summary>
        /// The internal buffer backing this <see cref="RingBuffer{T}"/>.
        /// </summary>
        public T[] InternalBuffer { get; }

        /// <summary>
        /// Creates a 
        /// </summary>
        /// <param name="bufferLength"></param>
        public RingBuffer(int bufferLength) {
            this.InternalBuffer = new T[bufferLength];
        }

        /// <summary>
        /// Pushes a value into the <see cref="RingBuffer{T}"/>.
        /// </summary>
        /// <param name="value">The value to push into this <see cref="RingBuffer{T}"/>.</param>
        public void PushValue(T value) {
            this.InternalBuffer[_ringIndex] = value;
            _ringIndex                      = (_ringIndex + 1) % this.InternalBuffer.Length;
        }

    }
}
