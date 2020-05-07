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

        private static readonly Mutex SingleInstanceMutex = new Mutex(true, "{5802208e-71ca-4745-ab1b-d851bc17a460}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Cli.Parse<ApplicationSettings>(args);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));

            EnableLogging();

            Logger.Debug("Launched from {launchDirectory} with args {launchOptions}.", Directory.GetCurrentDirectory(), string.Join(" ", args));

            if (IsMoreThanOneInstance()) {
                Logger.Warn("Blish HUD is already running!");
                return;
            }

            // Needed by textboxes to enable CTRL + A selection
            Application.EnableVisualStyles();

            using (var game = new BlishHud()) game.Run();
            SingleInstanceMutex.ReleaseMutex();
        }

        private static bool IsMoreThanOneInstance() => !SingleInstanceMutex.WaitOne(TimeSpan.Zero, true);

    }

}
