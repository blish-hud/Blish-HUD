using Blish_HUD.DebugHelper.Services;
using EntryPoint;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Blish_HUD {

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program {

        private static Logger Logger;

        private const string APP_GUID = "{5802208e-71ca-4745-ab1b-d851bc17a460}";

        public static SemVer.Version OverlayVersion { get; } = new SemVer.Version(typeof(BlishHud).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, true);

        internal static bool RestartOnExit { get; set; } = false;

        private static string[] _startupArgs;

        private static void EnableLogging() {
            // Make sure logging and logging services are available as soon as possible
            DebugService.InitDebug();
            Logger = Logger.GetLogger(typeof(Program));

            if (!string.IsNullOrEmpty(OverlayVersion.PreRelease)) {
                Logger.Info("Running PreRelease {preReleaseVersion}", OverlayVersion.PreRelease);
            }
            if (!string.IsNullOrEmpty(OverlayVersion.Build)) {
                Logger.Info("Running Build {build}", OverlayVersion.Build);
            }
        }

        private static void HandleArcDps11Contingency() {
            // Typically occurs when ArcDps is placed in the same directory as Blish HUD
            // and causes Blish HUD to crash almost immediately due to an access violation.
            if (File.Exists("d3d11.dll")) {
                Debug.Contingency.NotifyArcDpsSameDir();
            }
        }

        private static void HandleMinTls12Contingency() {
            // Typically occurs on Windows 7 where Tls 1.2 has not been configured as the default.
            // https://link.blishhud.com/tls12issue
            if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12)) {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args) {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));

            // TODO: Get SetDllDirectory("") working so that we can protect ourselves from this
            HandleArcDps11Contingency();
            HandleMinTls12Contingency();

            _startupArgs = args;
            var settings = Cli.Parse<ApplicationSettings>(args);

            if (settings.MainProcessId.HasValue) {
                // The only current subprocess is our DebugHelper
                RunDebugHelper(settings.MainProcessId.Value);
                return;
            }

            if (settings.CliExitEarly) return;

            EnableLogging();

            Logger.Info("Launched from {launchDirectory} with args {launchOptions}.", Directory.GetCurrentDirectory(), string.Join(" ", args));
            
            string mutexName = string.IsNullOrEmpty(ApplicationSettings.Instance.MumbleMapName) ? $"{APP_GUID}" : $"{APP_GUID}:{ApplicationSettings.Instance.MumbleMapName}";
            using (Mutex singleInstanceMutex = new Mutex(true, mutexName, out bool ownsMutex)) {
                try {
                    if (!ownsMutex) {
                        // we don't own the mutex - try to acquire.
                        try {
                            if (!(ownsMutex = singleInstanceMutex.WaitOne(TimeSpan.Zero, true))) {
                                Logger.Warn("Blish HUD is already running!");
                                return;
                            }
                        } catch (AbandonedMutexException ex) {
                            // log exception, but mutex still acquired
                            Logger.Warn(ex, "Caught AbandonedMutexException, previous Blish HUD instance terminated non-gracefully.");
                        }
                    }

                    using (var game = new BlishHud()) {
                        game.Run();
                    }

                } finally {
                    if (ownsMutex) {
                        // only release if we acquired ownership
                        // .Dispose() only closes the wait handle 
                        // and doesn't release the mutex - this can
                        // cause AbandonedMutexException next run
                        singleInstanceMutex.ReleaseMutex();
                    }

                    if (RestartOnExit) {
                        var currentStartInfo = Process.GetCurrentProcess().StartInfo;
                        currentStartInfo.FileName = Application.ExecutablePath;
                        currentStartInfo.Arguments = string.Join(" ", _startupArgs);

                        Process.Start(currentStartInfo);
                    }
                }
            }
        }

        private static void RunDebugHelper(int mainProcessId) {
            using var inStream = Console.OpenStandardInput();
            using var outStream = Console.OpenStandardOutput();

            var processService = new ProcessService(mainProcessId);
            using var messageService = new StreamMessageService(inStream, outStream);
            using var mouseHookService = new MouseHookService(messageService);
            using var keyboardHookService = new KeyboardHookService(messageService);
            using var inputManagerService = new InputManagerService(messageService, mouseHookService, keyboardHookService);

            processService?.Start();
            messageService.Start();
            mouseHookService.Start();
            keyboardHookService.Start();
            inputManagerService.Start();

            Application.Run();
        }

    }

}
