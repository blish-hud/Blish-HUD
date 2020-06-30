using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class InputService : GameService {

        private readonly IHookManager inputHookManager;

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
                inputHookManager = new DebugHelperHookManager();
            } else {
                inputHookManager = new WinApiHookManager();
            }
        }

        internal void EnableHooks() {
            if (inputHookManager.EnableHook()) {
                inputHookManager.RegisterMouseHandler(Mouse.HandleInput);
                inputHookManager.RegisterKeyboardHandler(Keyboard.HandleInput);
            }
        }

        internal void DisableHooks() {
            inputHookManager.DisableHook();
            inputHookManager.UnregisterMouseHandler(Mouse.HandleInput);
            inputHookManager.UnregisterKeyboardHandler(Keyboard.HandleInput);
        }

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            inputHookManager.Load();
            GameIntegration.Gw2AcquiredFocus += (s, e) => EnableHooks();
            GameIntegration.Gw2LostFocus += (s, e) => DisableHooks();
            GameIntegration.Gw2Closed += (s, e) => DisableHooks();
        }

        protected override void Unload() {
            DisableHooks();
            inputHookManager.Unload();
        }

        protected override void Update(GameTime gameTime) {
            Mouse.Update();
            Keyboard.Update();
        }
    }
}
