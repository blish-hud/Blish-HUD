using System;
using System.Collections.Generic;
using Blish_HUD.Contexts;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class ContextsService : GameService {

        /// <summary>
        /// Returned by the <see cref="ContextsService"/> when registering
        /// a new <see cref="Context"/>.  This handle can be used to later
        /// unregister the <see cref="Context"/> by calling <see cref="Expire"/>
        /// on the handle.
        /// </summary>
        public class ContextHandle<TContext> where TContext : Context {

            private bool _hasExpired;

            /// <summary>
            /// Unloads and invalidates the <see cref="Context"/> marking it
            /// as <see cref="ContextState.Expired"/>.  The <see cref="Context"/>
            /// is then unregistered from the <see cref="ContextsService"/>.
            /// </summary>
            public void Expire() {
                if (_hasExpired) return;

                _hasExpired = true;

                GameService.Contexts.UnregisterContext<TContext>();
            }

        }

        private readonly Dictionary<Type, Context> _registeredContexts = new Dictionary<Type, Context>();

        /// <inheritdoc />
        protected override void Initialize() {
            // Built-in contexts
            RegisterContext(new Gw2ClientContext());
            RegisterContext(new CdnInfoContext());
            RegisterContext(new FestivalContext());
        }

        /// <inheritdoc />
        protected override void Load() { /* NOOP */ }

        /// <summary>
        /// Registers and then loads an instance of a <see cref="Context"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of <see cref="Context"/> that will be registered.</typeparam>
        /// <param name="context">An instance of a <see cref="Context"/>.</param>
        /// <returns>A <see cref="ContextHandle{TContext}"/> which can be used to later unregister the <see cref="Context"/>.</returns>
        public ContextHandle<TContext> RegisterContext<TContext>(TContext context) where TContext : Context {
            context.DoLoad();

            _registeredContexts.Add(context.GetType(), context);

            return new ContextHandle<TContext>();
        }

        private void UnregisterContext<TContext>() {
            if (_registeredContexts.ContainsKey(typeof(TContext))) {
                var context = _registeredContexts[typeof(TContext)];

                _registeredContexts.Remove(typeof(TContext));

                context.DoUnload();
            }
        }

        /// <summary>
        /// Gets a registered <see cref="Context"/> by type.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="Context"/> to retrieve.</typeparam>
        /// <returns>
        /// The registered <see cref="Context"/> of type <c>TContext</c> or
        /// <c>null</c> if no <see cref="Context"/> of that type is
        /// currently registered.
        /// </returns>
        public TContext GetContext<TContext>() where TContext : Context {
            if (!_registeredContexts.ContainsKey(typeof(TContext))) return null;

            return _registeredContexts[typeof(TContext)] as TContext;
        }

        /// <summary>
        /// Gets a registered <see cref="Context"/> by interface or base class.
        /// </summary>
        /// <typeparam name="TBase">The type the <see cref="Context"/> implements.</typeparam>
        /// <returns>
        ///The registered <see cref="Context"/> that implments <c>TBase</c> or
        /// <c>null</c> if no <see cref="Context"/> that implements that type is
        /// currently registered.
        /// </returns>
        public TBase GetContextImplementing<TBase>() {
            foreach (var context in _registeredContexts) {
                if (context.Value is TBase baseContext) {
                    return baseContext;
                }
            }

            return default;
        }

        /// <inheritdoc />
        protected override void Unload() { /* NOOP */ }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime) { /* NOOP */ }

    }
}
