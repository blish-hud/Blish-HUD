using Blish_HUD._Extensions;
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
        private readonly ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public bool IsReadOnly { get; } = false;

        public bool IsEmpty {
            get => _innerList.Count == 0;
        }

        public ControlCollection() {
            _innerList = new List<T>();
        }

        public ControlCollection(IEnumerable<T> existingControls) {
            _innerList = new List<T>(existingControls);
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
            using (_listLock.EnterDisposableWriteLock()) {
                this.Insert(this.Count, item);
            }
        }

        public void AddRange(IEnumerable<T> items) {
            using (_listLock.EnterDisposableWriteLock()) {
                T[] newItems = items.ToArray();

                foreach (T item in newItems) {
                    this.Add(item);
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
                if (!this.Contains(item)) {
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
                if (value == null) {
                    return;
                }

                using (_listLock.EnterDisposableWriteLock()) {
                    int found = this.IndexOf(value);

                    if (found != -1 && found != index) {
                        _innerList[index] = value;
                    }
                }
            }
        }

        ~ControlCollection() {
            _listLock?.Dispose();
        }

    }
}
