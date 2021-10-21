using Blish_HUD.DebugHelper.Models;
using Blish_HUD.DebugHelper.Services;
using System;
using System.Diagnostics;
using System.Timers;

namespace Blish_HUD.Input {

    internal class DebugHelperHookManager : IHookManager {

        private static readonly Logger Logger = Logger.GetLogger<DebugHelperHookManager>();

        private IMouseHookManager    _mouseHookManager;
        private IKeyboardHookManager _keyboardHookManager;
        private Process              _process;
        private IMessageService      _debugHelperMessageService;
        private Timer                _pingTimer;
        private bool                 _isHookEnabled = false;

        public void Load() {
            Logger.Debug("Loading DebugHelper input hooks");

            using var currentProcess = Process.GetCurrentProcess();
            var processFileName = currentProcess.MainModule.FileName;

            _process = new Process {
                StartInfo = new ProcessStartInfo(processFileName, $"--mainprocessid {currentProcess.Id}") {
                    RedirectStandardInput  = true,
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                }
            };
            _process.Exited += Process_Exited;

            Logger.Debug("Starting subprocess: \"{FileName}\" {Arguments}", _process.StartInfo.FileName, _process.StartInfo.Arguments);
            _process.Start();

            _debugHelperMessageService = new StreamMessageService(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream);
            _debugHelperMessageService.Start();

            _pingTimer         =  new Timer(10) { AutoReset = true };
            _pingTimer.Elapsed += (s, e) => _debugHelperMessageService.Send(new PingMessage());
            _pingTimer.Start();

            _mouseHookManager    = new DebugHelperMouseHookManager(_debugHelperMessageService);
            _keyboardHookManager = new DebugHelperKeyboardHookManager(_debugHelperMessageService);
        }

        private void Process_Exited(object sender, EventArgs e) {
            Logger.Debug("Subprocess with id {ProcessId} has exited with exit code {ExitCode}", _process.Id, _process.ExitCode);
        }

        public void Unload() {
            Logger.Debug("Unloading DebugHelper input hooks");
            _debugHelperMessageService.Stop();
            _pingTimer.Stop();
            Logger.Debug("Killing subprocess with id {ProcessId}", _process.Id);
            if (!_process.HasExited) _process.Kill();
            _debugHelperMessageService = null;
            _process                   = null;
        }

        public bool EnableHook() {
            if (_isHookEnabled) return false;

            Logger.Debug("Enabling DebugHelper input hooks");

            _isHookEnabled = _mouseHookManager.EnableHook() && _keyboardHookManager.EnableHook();
            return _isHookEnabled;
        }

        public void DisableHook() {
            if (!_isHookEnabled) return;

            Logger.Debug("Disabling DebugHelper input hooks");

            _mouseHookManager.DisableHook();
            _keyboardHookManager.DisableHook();

            _isHookEnabled = false;
        }

        public void RegisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { _mouseHookManager.RegisterHandler(handleMouseInputCallback); }

        public void UnregisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { _mouseHookManager.UnregisterHandler(handleMouseInputCallback); }

        public void RegisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { _keyboardHookManager.RegisterHandler(handleKeyboardInputCallback); }

        public void UnregisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { _keyboardHookManager.UnregisterHandler(handleKeyboardInputCallback); }

        #region IDisposable Support

        private bool isDisposed = false;

        protected virtual void Dispose(bool isDisposing) {
            if (!isDisposed) {
                if (isDisposing) {
                    Unload();
                    _process?.Dispose();
                    _pingTimer?.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
