using Blish_HUD.DebugHelper.Models;
using Blish_HUD.DebugHelper.Services;
using System;
using System.Diagnostics;
using System.Timers;

namespace Blish_HUD.Input {

    internal class DebugHelperHookManager : IHookManager {

        private static readonly Logger Logger = Logger.GetLogger<DebugHelperHookManager>();

        private IMouseHookManager    mouseHookManager;
        private IKeyboardHookManager keyboardHookManager;
        private Process              process;
        private IMessageService      debugHelperMessageService;
        private Timer                pingTimer;
        private bool                 isHookEnabled = false;

        public void Load() {
            Logger.Debug("Loading DebugHelper input hooks");

            using var currentProcess = Process.GetCurrentProcess();
            var processFileName = currentProcess.MainModule.FileName;

            process = new Process {
                StartInfo = new ProcessStartInfo(processFileName, $"--mainprocessid {currentProcess.Id}") {
                    RedirectStandardInput  = true,
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                }
            };
            process.Exited += Process_Exited;

            Logger.Debug("Starting subprocess: \"{FileName}\" {Arguments}", process.StartInfo.FileName, process.StartInfo.Arguments);
            process.Start();

            debugHelperMessageService = new StreamMessageService(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            debugHelperMessageService.Start();

            pingTimer         =  new Timer(10) { AutoReset = true };
            pingTimer.Elapsed += (s, e) => debugHelperMessageService.Send(new PingMessage());
            pingTimer.Start();

            mouseHookManager    = new DebugHelperMouseHookManager(debugHelperMessageService);
            keyboardHookManager = new DebugHelperKeyboardHookManager(debugHelperMessageService);
        }

        private void Process_Exited(object sender, EventArgs e) {
            Logger.Debug("Subprocess with id {ProcessId} has exited with exit code {ExitCode}", process.Id, process.ExitCode);
        }

        public void Unload() {
            Logger.Debug("Unloading DebugHelper input hooks");
            debugHelperMessageService.Stop();
            pingTimer.Stop();
            Logger.Debug("Killing subprocess with id {ProcessId}", process.Id);
            if (!process.HasExited) process.Kill();
            debugHelperMessageService = null;
            process                   = null;
        }

        public bool EnableHook() {
            if (isHookEnabled) return false;

            Logger.Debug("Enabling DebugHelper input hooks");

            isHookEnabled = mouseHookManager.EnableHook() && keyboardHookManager.EnableHook();
            return isHookEnabled;
        }

        public void DisableHook() {
            if (!isHookEnabled) return;

            Logger.Debug("Disabling DebugHelper input hooks");

            mouseHookManager.DisableHook();
            keyboardHookManager.DisableHook();

            isHookEnabled = false;
        }

        public void RegisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { mouseHookManager.RegisterHandler(handleMouseInputCallback); }

        public void UnregisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { mouseHookManager.UnregisterHandler(handleMouseInputCallback); }

        public void RegisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { keyboardHookManager.RegisterHandler(handleKeyboardInputCallback); }

        public void UnregisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { keyboardHookManager.UnregisterHandler(handleKeyboardInputCallback); }

        #region IDisposable Support

        private bool isDisposed = false;

        protected virtual void Dispose(bool isDisposing) {
            if (!isDisposed) {
                if (isDisposing) {
                    Unload();
                    process?.Dispose();
                    pingTimer?.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
