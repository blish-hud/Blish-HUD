using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Blish_HUD.Controls {

    public class ControlCollection<T> : IList<T>
        where T : Control {

        // BREAKME: We'd prefer to not implement IList and instead use a
        // different interface, but it will be a breaking change. We should
        // revise this the next time we make a major breaking change.

        private readonly List<T>              _innerList;
        private readonly ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim();

        public bool IsReadOnly { get; } = false;

        public bool IsEmpty {
            get => _innerList.Count == 0;
        }

        public ControlCollection() {
            _innerList = new List<T>();
        }

        public ControlCollection(IEnumerable<T> existingControls) {
            T[] newItems = existingControls.Distinct().Where(control => control != null).ToArray();
            _innerList = new List<T>(newItems);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() {
            // create a copy for safe enumeration.
            return ((IEnumerable<T>)this.ToArray()).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public void Add(T item) {
            this.Insert(this.Count, item);
        }

        public void AddRange(IEnumerable<T> items) {
            T[] newItems = items.ToArray();

            using (_listLock.EnterDisposableWriteLock()) {
                EnsureCapacity(_innerList.Count + newItems.Length);

                foreach (T item in newItems) {
                    if (item != null && !_innerList.Contains(item)) {
                        _innerList.Add(item);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Clear() {
            using (_listLock.EnterDisposableWriteLock()) {
                _innerList.Clear();
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item) {
            return this.IndexOf(item) != -1;
        }

        /// <summary>
        /// Do not use.
        /// </summary>
        [Obsolete("Do not use. Throws an exception.")]
        public void CopyTo(T[] array, int arrayIndex) {
            throw new InvalidOperationException($"{nameof(CopyTo)} not supported.  If using LINQ, ensure you call .ToList or .ToArray directly on {nameof(ControlCollection<T>)} first.");
        }

        /// <inheritdoc/>
        public bool Remove(T item) {
            if (item == null) {
                return false;
            }

            using (_listLock.EnterDisposableWriteLock()) {
                return _innerList.Remove(item);
            }
        }

        /// <inheritdoc/>
        public int Count {
            get {
                using (_listLock.EnterDisposableReadLock()) {
                    return _innerList.Count;
                }
            }
        }

        public List<T> ToList() {
            using (_listLock.EnterDisposableReadLock()) {
                return new List<T>(_innerList);
            }
        }

        public T[] ToArray() {
            using (_listLock.EnterDisposableReadLock()) {
                return _innerList.ToArray();
            }
        }

        /// <inheritdoc/>
        public int IndexOf(T item) {
            if (item == null) {
                return -1;
            }

            using (_listLock.EnterDisposableReadLock()) {
                return _innerList.IndexOf(item);
            }
        }

        /// <inheritdoc/>
        public void Insert(int index, T item) {
            if (item == null) {
                return;
            }

            using (_listLock.EnterDisposableWriteLock()) {
                if (!_innerList.Contains(item)) {
                    _innerList.Insert(index, item);
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveAt(int index) {
            using (_listLock.EnterDisposableWriteLock()) {
                _innerList.RemoveAt(index);
            }
        }

        /// <inheritdoc/>
        public T this[int index] {
            get {
                using (_listLock.EnterDisposableReadLock()) {
                    return _innerList[index];
                }
            }
            set {
                using (_listLock.EnterDisposableWriteLock()) {
                    if (value == null) {
                        _innerList.RemoveAt(index);
                    } else {
                        int found = _innerList.IndexOf(value);

                        if (found != index) {
                            _innerList[index] = value;

                            if (found != -1) {
                                _innerList.RemoveAt(found);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the list has capacity for <paramref name="min"/> items.
        /// Must be used from a Write Lock.
        /// </summary>
        /// <param name="min"></param>
        private void EnsureCapacity(int min) {
            const int MAX_ARRAY_LENGTH = 0x7FEFFFFF;

            if (_innerList.Capacity < min) {
                int num = (_innerList.Capacity == 0) ? 4 : (_innerList.Capacity * 2);
                if ((uint)num > MAX_ARRAY_LENGTH) {
                    num = MAX_ARRAY_LENGTH;
                }

                if (num < min) {
                    num = min;
                }

                _innerList.Capacity = num;
            }
        }

        ~ControlCollection() {
            _listLock?.Dispose();
        }

    }
}
