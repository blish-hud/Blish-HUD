namespace Blish_HUD._Extensions {
    using System.Threading;

    internal static class ReaderWriterLockSlimExtensions {

        // These could be ref classes and use IDisposable,
        // but a ref struct is slightly more performant,
        // and prevents error-prone usage in async methods.

        public readonly ref struct DisposableReadLock {
            private readonly ReaderWriterLockSlim _rwl;

            public DisposableReadLock(ReaderWriterLockSlim rwl) {
                _rwl = rwl;
                _rwl.EnterReadLock();
            }

            public void Dispose() {
                _rwl.ExitReadLock();
            }
        }

        public readonly ref struct DisposableWriteLock {
            private readonly ReaderWriterLockSlim _rwl;

            public DisposableWriteLock(ReaderWriterLockSlim rwl) {
                _rwl = rwl;
                _rwl.EnterWriteLock();
            }

            public void Dispose() {
                _rwl.ExitWriteLock();
            }
        }

        public static DisposableReadLock EnterDisposableReadLock(this ReaderWriterLockSlim rwl) {
            return new DisposableReadLock(rwl);
        }

        public static DisposableWriteLock EnterDisposableWriteLock(this ReaderWriterLockSlim rwl) {
            return new DisposableWriteLock(rwl);
        }

    }
}
