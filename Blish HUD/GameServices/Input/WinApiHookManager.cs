using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Blish_HUD.Input {

    internal class WinApiHookManager : IHookManager {

        private static readonly Logger Logger = Logger.GetLogger<WinApiHookManager>();

        private readonly IMouseHookManager    mouseHookManager;
        private readonly IKeyboardHookManager keyboardHookManager;
        private readonly AutoResetEvent       inputHookEvent = new AutoResetEvent(false);
        private          bool                 stopRequested  = false;
        private          Thread               thread;
        private          bool                 inputSuccessful = false;

        public WinApiHookManager() {
            mouseHookManager    = new WinApiMouseHookManager();
            keyboardHookManager = new WinApiKeyboardHookManager();
        }

        public void Load() { /* NOOP */ }

        public void Unload() => DisableHook();

        public bool EnableHook() {
            if (thread != null) return false;

            Logger.Debug("Enabling WinAPI input hooks");

            thread = new Thread(Loop);
            thread.Start();

            // Wait for the hook to be completed and return the status
            inputHookEvent.WaitOne();
            return inputSuccessful;
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

            if (mouseHookManager.EnableHook() && keyboardHookManager.EnableHook()) inputSuccessful = true;
            inputHookEvent.Set();

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
                if (isDisposing) {
                    inputHookEvent.Dispose();
                    Unload();
                }

                isDisposed = true;
            }
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
