using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Contexts {

    /// <summary>
    /// The current state of the <see cref="Context"/>.
    /// </summary>
    public enum ContextState {
        /// <summary>
        /// The <see cref="Context"/> is currently loading.
        /// </summary>
        Loading,

        /// <summary>
        /// The <see cref="Context"/> has loaded.
        /// </summary>
        Ready,

        /// <summary>
        /// The <see cref="Context"/> has been unregistered and should no longer be used or referenced.
        /// </summary>
        Expired
    }

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

        private ContextState _state = ContextState.Loading;

        /// <summary>
        /// The current load state of the <see cref="Context"/>.
        /// </summary>
        public ContextState State {
            get => _state;
            set {
                if (_state == value) return;

                _state = value;

                OnStateChanged(EventArgs.Empty);
            }
        }

        public void DoLoad() {
            this.State = ContextState.Loading;

            this.Load();
        }

        public void DoUnload() {
            this.State = ContextState.Expired;

            this.Unload();
        }

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
            contextResult = new ContextResult<T>(default, Properties.Strings.Context_StateNotReady);

            return ContextAvailability.NotReady;
        }

        protected virtual void Load() { /* NOOP */ }

        protected virtual void Unload() { /* NOOP */ }

    }

}