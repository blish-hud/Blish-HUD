using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Contexts;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class ContextsService : GameService {

        private readonly Dictionary<Type, Context> _registeredContexts = new Dictionary<Type, Context>();

        /// <inheritdoc />
        protected override void Initialize() {
            // Temporary register
            RegisterContext(new Gw2ClientContext());
            RegisterContext(new CdnInfoContext());
        }

        /// <inheritdoc />
        protected override void Load() { /* NOOP */ }

        public void RegisterContext(Context context) {
            context.DoLoad();

            _registeredContexts.Add(context.GetType(), context);
        }

        public void UnregisterContext<TContext>() {
            if (_registeredContexts.ContainsKey(typeof(TContext))) {
                var context = _registeredContexts[typeof(TContext)];

                _registeredContexts.Remove(typeof(TContext));

                context.DoUnload();
            }
        }

        public TContext GetContext<TContext>() where TContext : class {
            if (!_registeredContexts.ContainsKey(typeof(TContext))) return null;

            return _registeredContexts[typeof(TContext)] as TContext;
        }

        /// <inheritdoc />
        protected override void Unload() { /* NOOP */ }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime) {
            foreach (var context in _registeredContexts.Values) {
                context.DoUpdate(gameTime);
            }
        }

    }
}
