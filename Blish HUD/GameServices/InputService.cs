using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class InputService : GameService {

        private readonly IHookManager hookManager;

        /// <summary>
        /// Provides details about the current mouse state.
        /// </summary>
        public MouseHandler Mouse { get; }

        /// <summary>
        /// Provides details about the current keyboard state.
        /// </summary>
        public KeyboardHandler Keyboard { get; }

        public InputService() {
            Mouse = new MouseHandler();
            Keyboard = new KeyboardHandler();

            if (ApplicationSettings.Instance.DebugEnabled) {
                hookManager = new DebugHelperHookManager();
            } else {
                hookManager = new WinApiHookManager();
            }
        }

        internal void EnableHooks() {
            if (hookManager.EnableHook()) {
                hookManager.RegisterMouseHandler(Mouse.HandleInput);
                hookManager.RegisterKeyboardHandler(Keyboard.HandleInput);
            }
        }

        internal void DisableHooks() {
            hookManager.DisableHook();
            hookManager.UnregisterMouseHandler(Mouse.HandleInput);
            hookManager.UnregisterKeyboardHandler(Keyboard.HandleInput);
        }

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            hookManager.Load();
            GameIntegration.Gw2AcquiredFocus += (s, e) => EnableHooks();
            GameIntegration.Gw2LostFocus += (s, e) => DisableHooks();
            GameIntegration.Gw2Closed += (s, e) => DisableHooks();
        }

        protected override void Unload() {
            DisableHooks();
            hookManager.Unload();
        }

        protected override void Update(GameTime gameTime) {
            Mouse.Update();
            Keyboard.Update();
        }
    }
}
