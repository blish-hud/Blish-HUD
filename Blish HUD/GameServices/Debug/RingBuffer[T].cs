namespace Blish_HUD.Debug {
    /// <summary>
    /// [NOT THREAD-SAFE] A fixed capacity buffer which overwrites itself as the index wraps.
    /// </summary>
    /// <typeparam name="T">The <c>Type</c> the <see cref="RingBuffer{T}"/> contains.</typeparam>
    public class RingBuffer<T> {

        protected int _ringIndex = 0;

        /// <summary>
        /// The array backing this buffer.
        /// </summary>
        public T[] InternalBuffer { get; }

        /// <summary>
        /// The length of the buffer.
        /// </summary>
        public int BufferLength => this.InternalBuffer.Length;

        /// <summary>
        /// Creates a ring buffer of size <paramref name="bufferLength"/>.
        /// </summary>
        /// <param name="bufferLength">The size of the buffer.</param>
        public RingBuffer(int bufferLength) {
            this.InternalBuffer = new T[bufferLength];
        }

        /// <summary>
        /// Pushes a value into the <see cref="RingBuffer{T}"/>.
        /// </summary>
        /// <param name="value">The value to push into this <see cref="RingBuffer{T}"/>.</param>
        public virtual void PushValue(T value) {
            this.InternalBuffer[_ringIndex] = value;

            _ringIndex = (_ringIndex + 1) % this.BufferLength;
        }

        public T this[int index] {
            get {
                if (index < 0) {
                    index = this.BufferLength + (index % this.BufferLength);
                }

                return this.InternalBuffer[index % this.BufferLength];
            }
        }

    }
}
