using Microsoft.Xna.Framework;
using Blish_HUD.Input;

namespace Blish_HUD {
    public class InputService : GameService {
        
        /// <summary>
        /// Provides details about the current mouse state.
        /// </summary>
        public MouseManager Mouse { get; }

        /// <summary>
        /// Provides details about the current keyboard state.
        /// </summary>
        public KeyboardManager Keyboard { get; }

        public InputService() {
            Mouse    = new MouseManager();
            Keyboard = new KeyboardManager();
        }

        internal void EnableHooks() {
            Mouse.Enable();
            Keyboard.Enable();
        }

        internal void DisableHooks() {
            Mouse.Disable();
            Keyboard.Disable();
        }

        protected override void Initialize() {
            EnableHooks();
        }

        protected override void Load() {
            GameIntegration.Gw2AcquiredFocus += delegate { EnableHooks(); };
            GameIntegration.Gw2LostFocus     += delegate { DisableHooks(); };
        }

        protected override void Unload() {
            DisableHooks();
        }

        protected override void Update(GameTime gameTime) {
            Mouse.Update();
            Keyboard.Update();
        }

    }
}
