using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Contexts {

    /// <summary>
    /// Provides context about something.
    /// </summary>
    public abstract class Context {

        /// <summary>
        /// Occurs when <see cref="State"/> changes.
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        protected void OnStateChanged(EventArgs e) {
            this.StateChanged?.Invoke(this, e);
        }

        private ContextState _state = ContextState.None;

        /// <summary>
        /// Gets the current load state of the <see cref="Context"/>.
        /// </summary>
        public ContextState State {
            get => _state;
            private set {
                if (_state == value || _state == ContextState.Expired) return;

                _state = value;

                OnStateChanged(EventArgs.Empty);
            }
        }

        public void DoLoad() {
            if (this.State == ContextState.Expired) return;

            this.State = ContextState.Loading;

            this.Load();
        }

        public void DoUnload() {
            this.State = ContextState.Expired;

            this.Unload();
        }

        /// <summary>
        /// Called to confirm that the context is now <see cref="ContextState.Ready"/>.
        /// </summary>
        protected void ConfirmReady() {
            this.State = ContextState.Ready;
        }

        /// <summary>
        /// If the <see cref="State"/> is not <see cref="ContextState.Ready"/> and a function is called
        /// on the <see cref="Context"/> that relies on something to be loaded, this short-circuit can
        /// be called to return <see cref="ContextAvailability.NotReady"/> and out the default result
        /// with the status set to "not ready".
        /// </summary>
        protected ContextAvailability NotReady<T>(out ContextResult<T> contextResult) {
            contextResult = new ContextResult<T>(default, Strings.GameServices.ContextsService.State_NotReady);

            return ContextAvailability.NotReady;
        }

        protected virtual void Load() { /* NOOP */ }

        protected virtual void Unload() { /* NOOP */ }

    }

}