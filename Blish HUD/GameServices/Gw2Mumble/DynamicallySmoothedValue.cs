using System;
using System.Linq.Expressions;
using Blish_HUD.Debug;

namespace Blish_HUD.Gw2Mumble {
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

        private readonly Func<int> _getBufferLengthFunc;

        private readonly Func<T, T, T>     _addExpression;
        private readonly Func<T, float, T> _divideExpression;

        /// <summary>
        /// Creates a value which can be smoothed based on a resizable buffer with a max length specified by <paramref name="bufferMax"/>.
        /// </summary>
        /// <param name="bufferMax">The maximum length of the buffer.</param>
        /// <param name="getBufferLengthFunc">The function used to decide how much of the buffer should be used to calculate the average.</param>
        public DynamicallySmoothedValue(int bufferMax, Func<int> getBufferLengthFunc = null) : base(bufferMax) {
            _getBufferLengthFunc = getBufferLengthFunc ?? (() => bufferMax);

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

        public override void PushValue(T value) {
            base.PushValue(value);

            _cachedValue = null;
        }

        private T GetAverageValue() {
            int currentSize = _getBufferLengthFunc();

            T total = default;

            for (int i = 1; i < currentSize + 1; i++) {
                total = _addExpression(total, this[_ringIndex - currentSize + i]);
            }

            return _divideExpression(total, currentSize);
        }

    }
}
