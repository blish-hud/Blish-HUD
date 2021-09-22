using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    public class InputService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<InputService>();

        private readonly IHookManager _hookManager;

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
                _hookManager = new DebugHelperHookManager();
            } else {
                _hookManager = new WinApiHookManager();
            }
        }

        internal void EnableHooks() {
            if (_hookManager.EnableHook()) {
                _hookManager.RegisterMouseHandler(Mouse.HandleInput);
                _hookManager.RegisterKeyboardHandler(Keyboard.HandleInput);
            } else {
                Logger.Error("Failed to acquire hook!");
            }
        }

        internal void DisableHooks() {
            _hookManager.DisableHook();
            _hookManager.UnregisterMouseHandler(Mouse.HandleInput);
            _hookManager.UnregisterKeyboardHandler(Keyboard.HandleInput);
        }

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            _hookManager.Load();
            GameIntegration.Gw2Instance.Gw2AcquiredFocus += (s, e) => EnableHooks();
            GameIntegration.Gw2Instance.Gw2LostFocus     += (s, e) => DisableHooks();
            GameIntegration.Gw2Instance.Gw2Closed        += (s, e) => DisableHooks();
        }

        protected override void Unload() {
            DisableHooks();
            _hookManager.Unload();
        }

        protected override void Update(GameTime gameTime) {
            Mouse.Update();
            Keyboard.Update();
        }
    }
}
