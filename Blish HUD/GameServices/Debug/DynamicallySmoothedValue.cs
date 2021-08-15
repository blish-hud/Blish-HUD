using System;
using System.Linq.Expressions;

namespace Blish_HUD.Debug {
    /// <summary>
    /// [NOT THREAD-SAFE] A ring buffer which can return an average using a dynamic buffer length.
    /// </summary>
    /// <typeparam name="T">The <c>Type</c> the <see cref="DynamicallySmoothedValue{T}"/> smooths.</typeparam>
    public class DynamicallySmoothedValue<T> : RingBuffer<T>
        where T : struct {

        private T? _cachedValue;

        /// <summary>
        /// The smoothed/averaged value.
        /// </summary>
        public T Value => _cachedValue ??= GetAverageValue();

        private int _currentSize;

        private readonly Func<T, T, T>     _addExpression;
        private readonly Func<T, float, T> _divideExpression;

        /// <summary>
        /// Creates a value which can be smoothed based on a resizable buffer with a max length specified by <paramref name="bufferMax"/>.
        /// </summary>
        /// <param name="bufferMax">The maximum length of the buffer.</param>
        /// <param name="getBufferLengthFunc">The function used to decide how much of the buffer should be used to calculate the average.</param>
        public DynamicallySmoothedValue(int bufferMax) : base(bufferMax) {
            _currentSize = 0;
            _addExpression    = PrepareAddExpression<T>();
            _divideExpression = PrepareDivideExpression<T>();
        }

        private static Func<TParam, TParam, TParam> PrepareAddExpression<TParam>() {
            var paramA = Expression.Parameter(typeof(TParam));
            var paramB = Expression.Parameter(typeof(TParam));

            return Expression.Lambda<Func<TParam, TParam, TParam>>(Expression.Add(paramA, paramB),
                                                                   paramA,
                                                                   paramB).Compile();
        }

        private static Func<TParam, float, TParam> PrepareDivideExpression<TParam>() {
            var paramA = Expression.Parameter(typeof(TParam));
            var paramB = Expression.Parameter(typeof(float));

            return Expression.Lambda<Func<TParam, float, TParam>>(Expression.Divide(paramA, paramB),
                                                                  paramA,
                                                                  paramB).Compile();
        }

        public void flush() {
            _ringIndex = 0;
            _currentSize = 0;
        }

        public override void PushValue(T value) {
            base.PushValue(value);

            if (_currentSize < BufferLength)
                _currentSize++;

            _cachedValue = null;
        }

        private T GetAverageValue() {
            T total = default;

            for (int i = 0; i < _currentSize; i++) {
                total = _addExpression(total, this[_ringIndex - i - 1]);
            }

            return _divideExpression(total, _currentSize);
        }

    }
}
