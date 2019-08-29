using System;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Contexts {
    /// <summary>
    /// Provides context about something
    /// </summary>
    public abstract class Context {

        /// <summary>
        /// Occurs when the context has finished loading.
        /// </summary>
        public event EventHandler<EventArgs> Readied;

        private void OnReadied(EventArgs e) {
            this.Readied?.Invoke(this, e);
        }

        public bool Ready { get; private set; }

        public void DoLoad() {
            this.Ready = false;

            this.Load();
        }

        public void DoUpdate(GameTime gameTime) {
            this.Update(gameTime);
        }

        public void DoUnload() {
            this.Ready = false;

            this.Unload();
        }

        protected void ConfirmReady() {
            this.Ready = true;

            this.OnReadied(EventArgs.Empty);
        }

        /// <summary>
        /// If the <see cref="Context"/> is not <see cref="Context.Ready"/> and a function is called
        /// on the <see cref="Context"/> that relies on something to be loaded, this short-circuit can
        /// be called to return <see cref="ContextAvailability.NotReady"/> and out the default result
        /// with the status set to "Not ready!"
        /// </summary>
        protected ContextAvailability NotReady<T>(out ContextResult<T> contextResult) {
            contextResult = new ContextResult<T>(default, "Not ready!");

            return ContextAvailability.NotReady;
        }

        protected virtual void Load() { /* NOOP */ }

        protected virtual void Update(GameTime gameTime) { /* NOOP */ }

        protected virtual void Unload() { /* NOOP */ }

    }

}