using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace Blish_HUD.GameServices {
    public abstract class ServiceModule<T> : IUpdatable
        where T : GameService {

        protected readonly T _service;

        protected ServiceModule(T service) {
            _service = service;
        }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

        internal virtual void Unload() { /* NOOP */ }

    }
}
