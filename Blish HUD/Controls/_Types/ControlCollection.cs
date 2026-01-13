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

        private class ControlEnumerator<TEnum> : IEnumerator<TEnum> {

            private readonly IEnumerator<TEnum>   _inner;
            private readonly ReaderWriterLockSlim _rwLock;

            public ControlEnumerator(IEnumerator<TEnum> inner, ReaderWriterLockSlim rwLock) {
                _inner  = inner;
                _rwLock = rwLock;
            }

            public bool MoveNext() {
                return _inner.MoveNext();
            }

            public void Reset() {
                _inner.Reset();
            }

            public object Current => _inner.Current;

            TEnum IEnumerator<TEnum>.Current => _inner.Current;

            public void Dispose() {
                if (_rwLock.IsReadLockHeld)
                    _rwLock.ExitReadLock();
            }
        }

        private readonly List<T>              _innerList;
        private readonly ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim();

        public bool IsReadOnly => false;

        public bool IsEmpty { get; private set; } = true;

        public ControlCollection() {
            _innerList = new List<T>();
        }

        public ControlCollection(IEnumerable<T> existingControls) {
            _innerList = new List<T>(existingControls);

            this.IsEmpty = !_innerList.Any();
        }

        public IEnumerator<T> GetEnumerator() {
            if (!_listLock.IsReadLockHeld)
                _listLock.EnterReadLock();

            return new ControlEnumerator<T>(_innerList.GetEnumerator(), _listLock);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            if (this.Contains(item) || item == null) return;
            
            if (!_listLock.IsWriteLockHeld)
                _listLock.EnterWriteLock();
            _innerList.Add(item);
            this.IsEmpty = false;
            _listLock.ExitWriteLock();
        }

        public void AddRange(IEnumerable<T> items) {
            if (!_listLock.IsWriteLockHeld)
                _listLock.EnterWriteLock();
            _innerList.AddRange(items);
            this.IsEmpty = !_innerList.Any();
            _listLock.ExitWriteLock();
        }

        public void Clear() {
            T[] oldItems = this.ToArray();

            if (!_listLock.IsWriteLockHeld)
                _listLock.EnterWriteLock();
            _innerList.Clear();
            this.IsEmpty = true;
            _listLock.ExitWriteLock();
            
            foreach (var item in oldItems) {
                item.Parent = null;
            }
        }

        public bool Contains(T item) {
            if (!_listLock.IsReadLockHeld)
                _listLock.EnterReadLock();

            try {
                return _innerList.Contains(item);
            } finally {
                _listLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Do not use.
        /// </summary>
        [Obsolete("Do not use. Throws an exception.")]
        public void CopyTo(T[] array, int arrayIndex) {
            throw new InvalidOperationException($"{nameof(CopyTo)} not supported.  If using LINQ, ensure you call .ToList or .ToArray directly on {nameof(ControlCollection<T>)} first.");
        }

        public bool Remove(T item) {
            if (!_listLock.IsWriteLockHeld)
                _listLock.EnterWriteLock();

            try {
                return _innerList.Remove(item);
            } finally {
                this.IsEmpty = !_innerList.Any();
                _listLock.ExitWriteLock();
            }
        }

        public int Count {
            get {
                if (!_listLock.IsReadLockHeld)
                    _listLock.EnterReadLock();

                try {
                    return _innerList.Count;
                } finally {
                    _listLock.ExitReadLock();
                }
            }
        }

        public List<T> ToList() {
            if (!_listLock.IsReadLockHeld)
                _listLock.EnterReadLock();

            try {
                return new List<T>(_innerList);
            } finally {
                _listLock.ExitReadLock();
            }
        }

        public T[] ToArray() {
            if (!_listLock.IsReadLockHeld)
                _listLock.EnterReadLock();

            try {
                var items = new T[_innerList.Count];
                _innerList.CopyTo(items, 0);
                return items;
            } finally {
                _listLock.ExitReadLock();
            }
        }

        public int IndexOf(T item) {
            if (!_listLock.IsReadLockHeld)
                _listLock.EnterReadLock();

            try {
                return _innerList.Count;
            } finally {
                _listLock.ExitReadLock();
            }
        }

        public void Insert(int index, T item) {
            if (!_listLock.IsWriteLockHeld)
                _listLock.EnterWriteLock();
            _innerList.Insert(index, item);
            _listLock.ExitWriteLock();
        }

        public void RemoveAt(int index) {
            if (!_listLock.IsWriteLockHeld)
                _listLock.EnterWriteLock();
            _innerList.RemoveAt(index);
            _listLock.ExitWriteLock();
        }

        public T this[int index] {
            get {
                if (!_listLock.IsReadLockHeld)
                    _listLock.EnterReadLock();

                try {
                    return _innerList[index];
                } finally {
                    _listLock.ExitReadLock();
                }
            }
            set {
                if (!_listLock.IsWriteLockHeld)
                    _listLock.EnterWriteLock();
                _innerList[index] = value;
                _listLock.ExitWriteLock();
            }
        }

        ~ControlCollection() {
            _listLock?.Dispose();
        }

    }
}
