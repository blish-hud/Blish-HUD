using System;
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

        public static SemVer.Version OverlayVersion { get; } = new SemVer.Version(typeof(BlishHud).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, true);

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
        private static void Main(string[] args) {
            Cli.Parse<ApplicationSettings>(args);

            EnableLogging();

            // Single instance handling
            _singleInstanceMutex = new Mutex(true, ApplicationSettings.Instance.MumbleMapName != null 
                                                       ? $"{APP_GUID}:{ApplicationSettings.Instance.MumbleMapName}"
                                                       : $"{APP_GUID}");

            if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true)) {
                Logger.Warn("Blish HUD is already running!");
                return;
            }

            // Needed by textboxes to enable CTRL + A selection
            Application.EnableVisualStyles();

            using (var game = new BlishHud()) {
                game.Run();
            }

            _singleInstanceMutex.ReleaseMutex();
        }

    }

}
