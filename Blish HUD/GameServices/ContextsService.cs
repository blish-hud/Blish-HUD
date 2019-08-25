using System;
using System.Collections.Generic;
using Blish_HUD.Contexts;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class ContextsService : GameService {

        private readonly Dictionary<Type, Context> _registeredContexts = new Dictionary<Type, Context>();

        /// <inheritdoc />
        protected override void Initialize() {
            
        }

        /// <inheritdoc />
        protected override void Load() {
            // Temporary register
            RegisterContext(new Gw2ClientContext());
            RegisterContext(new CdnInfoContext());
        }

        public void RegisterContext(Context context) {
            context.DoLoad();

            _registeredContexts.Add(context.GetType(), context);
        }

        public TContext GetContext<TContext>() where TContext : class {
            if (!_registeredContexts.ContainsKey(typeof(TContext))) return null;

            return _registeredContexts[typeof(TContext)] as TContext;
        }

        /// <inheritdoc />
        protected override void Unload() {
            
        }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime) {
            
        }

    }
}
