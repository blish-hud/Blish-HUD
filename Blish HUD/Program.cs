using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EntryPoint;

namespace Blish_HUD {

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program {

        private static Logger Logger;

        private const string APP_GUID = "{5802208e-71ca-4745-ab1b-d851bc17a460}";

        public static SemVer.Version OverlayVersion { get; } = new SemVer.Version("0.8.0");  //new SemVer.Version(typeof(BlishHud).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, true);

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

        private static Mutex _singleInstanceMutex;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            if (Cli.Parse<ApplicationSettings>(args).CliExitEarly) return;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));

            EnableLogging();

            Logger.Debug("Launched from {launchDirectory} with args {launchOptions}.", Directory.GetCurrentDirectory(), string.Join(" ", args));
            
            SelfUpdateUtil.TryHandleUpdate();

            if (!ApplicationSettings.Instance.RestartSkipMutex) {
                // Single instance handling
                _singleInstanceMutex = new Mutex(true, ApplicationSettings.Instance.MumbleMapName != null
                                                           ? $"{APP_GUID}:{ApplicationSettings.Instance.MumbleMapName}"
                                                           : $"{APP_GUID}");

                if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true)) {
                    Logger.Warn("Blish HUD is already running!");
                    return;
                }
            } else {
                Logger.Info("Skipping mutex for requested restart.");
            }

            using (var game = new BlishHud()) {
                game.Run();
            }

            if (!ApplicationSettings.Instance.RestartSkipMutex) {
                _singleInstanceMutex.ReleaseMutex();
            }
        }

    }

}
