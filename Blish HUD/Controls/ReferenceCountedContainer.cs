using System.Threading;

namespace Blish_HUD.Controls {
    public interface IReferenceCountedObject {
        void IncrementReference();
        void DecrementReference();
        bool IsSafeToDispose();
    }

    public abstract class ReferenceCountedContainer : Container, IReferenceCountedObject {
        private int _referenceCount;

        public void IncrementReference() {
            Interlocked.Increment(ref _referenceCount);
        }

        public void DecrementReference() {
            //Something went wrong and the reference count was not incremented
            if(_referenceCount == 0) return;

            if (Interlocked.Decrement(ref _referenceCount) == 0) {
                Dispose();
            }
        }

        public bool IsSafeToDispose() {
            return _referenceCount == 0;
        }

        protected override void DisposeControl() {
            if (!IsSafeToDispose())
                return;

            base.DisposeControl();
        }
    }
}
