using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Blish_HUD.Input {

    internal class WinApiInputHookManager : IInputHookManager {

        private static readonly Logger Logger = Logger.GetLogger<WinApiInputHookManager>();

        private readonly IMouseHookManager    mouseHookManager;
        private readonly IKeyboardHookManager keyboardHookManager;
        private          bool                 stopRequested = false;
        private          Thread               thread;

        public WinApiInputHookManager() {
            mouseHookManager    = new WinApiMouseHookManager();
            keyboardHookManager = new WinApiKeyboardHookManager();
        }

        public void Load() { }

        public void Unload() { DisableHook(); }

        public bool EnableHook() {
            if (thread != null) return false;

            Logger.Debug("Enabling WinAPI input hooks");

            if (!mouseHookManager.EnableHook() || !keyboardHookManager.EnableHook()) return false;

            thread = new Thread(new ThreadStart(Loop));
            thread.Start();
            return true;
        }

        public void DisableHook() {
            if ((thread == null) || stopRequested) return;

            Logger.Debug("Disabling WinAPI input hooks");

            mouseHookManager.DisableHook();
            keyboardHookManager.DisableHook();

            stopRequested = true;
            thread.Join();

            stopRequested = false;
            thread        = null;
        }

        public void RegisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { mouseHookManager.RegisterHandler(handleMouseInputCallback); }

        public void UnregisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { mouseHookManager.UnregisterHandler(handleMouseInputCallback); }

        public void RegisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { keyboardHookManager.RegisterHandler(handleKeyboardInputCallback); }

        public void UnregisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { keyboardHookManager.UnregisterHandler(handleKeyboardInputCallback); }

        private void Loop() {
            using Timer timer = new Timer {
                Interval = 10
            };

            timer.Tick += (sender, e) => {
                if (stopRequested) {
                    Logger.Debug("Stopping message loop");
                    Application.ExitThread();
                }
            };

            timer.Start();

            Logger.Debug("Starting message loop");

            // Start the message loop
            Application.Run();
            Logger.Debug("Message loop stopped");
        }

        #region IDisposable Support

        private bool isDisposed = false;

        protected virtual void Dispose(bool isDisposing) {
            if (!isDisposed) {
                if (isDisposing) Unload();

                isDisposed = true;
            }
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
