using System;
using System.Diagnostics;
using System.Windows.Forms;
using EntryPoint;
using EntryPoint.Exceptions;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Blish_HUD {
    [Help("Optional launch arguments that modify overlay behavior.")]
    public class ApplicationSettings : BaseCliArguments {

        private static ApplicationSettings _instance;

        internal static ApplicationSettings Instance => _instance;

        public bool CliExitEarly => this.UserFacingExceptionThrown || this.HelpInvoked;

        public ApplicationSettings() : base("Blish HUD") {
            _instance ??= this;

            InitDebug();
        }

        public override void OnUserFacingException(UserFacingException e, string message) {
            MessageBox.Show($"Invalid launch option(s) specified.  See --help for available options.\r\n\r\n{e.Message}", "Failed to launch Blish HUD", MessageBoxButtons.OK);
        }

        public override void OnHelpInvoked(string helpText) {
            MessageBox.Show(helpText, "Launch Options", MessageBoxButtons.OK);
        }

        [Conditional("DEBUG")]
        private void InitDebug() {
            this.DebugEnabled = true;
        }

        /*
         * d, debug     - Launches Blish HUD in debug mode.
         * f, maxfps    - The frame rate Blish HUD should target when rendering.
         * F, unlockfps - Unlocks the frame limit allowing Blish HUD to render as fast as possible.  This will cause higher CPU and GPU utilization.
         * m, mumble    - The MumbleLink map name to be used.
         * M, module    - The path to a module (*.bhm) that will be force loaded when Blish HUD launches.
         * p, process   - The name of the process to overlay (without '.exe').
         * P, pid       - The PID of the process to overlay.
         * r, ref       - The path to the ref.dat file.
         * s, settings  - The path where Blish HUD will save settings and other files.
         * w, window    - The name of the window to overlay.
         *
         * restartskipmutex - Forces Blish HUD to allow multiple instances.  Used internally to prevent race condition issues when Blish HUD is restarted.
         * handleupdate     - Blish HUD checks and unpacks a provided zip file 
         */

        #region Game Integration

        public const string OPTION_PROCESSID = "pid";
        /// <summary>
        /// The PID of the process to overlay.
        /// </summary>
        [
            OptionParameter(OPTION_PROCESSID, 'P'),
            Help("The PID of the process to overlay.")
        ]
        public int ProcessId { get; private set; } = 0;

        public const string OPTION_PROCESSNAME = "process";
        /// <summary>
        /// The name of the process to overlay (without '.exe').
        /// </summary>
        [
            OptionParameter(OPTION_PROCESSNAME, 'p'),
            Help("The name of the process to overlay (without '.exe').")
        ]
        public string ProcessName { get; private set; }

        public const string OPTION_WINDOWNAME = "window";
        /// <summary>
        /// The name of the window to overlay.
        /// </summary>
        [
            OptionParameter(OPTION_WINDOWNAME, 'w'),
            Help("The name of the window to overlay.")
        ]
        public string WindowName { get; private set; }

        public const string MUMBLEMAPNAME = "mumble";
        /// <summary>
        /// The MumbleLink map name to be used.
        /// </summary>
        [
            OptionParameter(MUMBLEMAPNAME, 'm'),
            Help("The MumbleLink map name to be used.")
        ]
        public string MumbleMapName { get; private set; }

        #endregion

        #region Utility

        public const string OPTION_USERSETTINGSPATH = "settings";
        /// <summary>
        /// The path where Blish HUD will save settings and other files.
        /// </summary>
        [
            OptionParameter(OPTION_USERSETTINGSPATH, 's'),
            Help("The path where Blish HUD will save settings and other files.")
        ]
        public string UserSettingsPath { get; private set; }

        public const string OPTION_REFPATH = "ref";
        /// <summary>
        /// The path to the ref.dat file.
        /// </summary>
        [
            OptionParameter("ref", 'r'),
            Help("The path to the ref.dat file.")
        ]
        public string RefPath { get; private set; }

        public const string OPTION_TARGETFRAMERATE = "maxfps";
        /// <summary>
        /// The frame rate Blish HUD should target when rendering.
        /// </summary>
        [
            OptionParameter(OPTION_TARGETFRAMERATE, 'f'),
            Help("The frame rate Blish HUD should target when rendering.")
        ]
        public double TargetFramerate { get; private set; } = -1;

        public const string OPTION_UNLOCKFPS = "unlockfps";
        /// <summary>
        /// Unlocks the frame limit allowing Blish HUD to render as fast as possible.  This will cause higher CPU and GPU utilization.
        /// </summary>
        [
            Option(OPTION_UNLOCKFPS, 'F'),
            Help("Deprecated as of v0.8.0.  Instead use the 'Frame Limiter' setting found in the Blish HUD graphics settings section.")
        ]
        [Obsolete("Deprecated as of v0.8.0.  Instead use the 'Frame Limiter' setting found in the Blish HUD graphics settings section.")]
        public bool UnlockFps { get; private set; }

        #endregion

        #region Debug

        public const string OPTION_DEBUGENABLED = "debug";
        /// <summary>
        /// Launches Blish HUD in debug mode.
        /// </summary>
        [
            Option(OPTION_DEBUGENABLED, 'd'),
            Help("Launches Blish HUD in debug mode.")
        ]
        public bool DebugEnabled { get; private set; }

        public const string OPTION_DEBUGMODULEPATH = "module";
        /// <summary>
        /// The path to a module (*.bhm) that will be force loaded when Blish HUD launches.
        /// </summary>
        [
            OptionParameter(OPTION_DEBUGMODULEPATH, 'M'),
            Help("The path to a module (*.bhm) that will be force loaded when Blish HUD launches.")
        ]
        public string DebugModulePath { get; private set; }

        #endregion

        #region Internal
        
        public const string OPTION_RESTARTSKIPMUTEX = "restartskipmutex";
        /// <summary>
        /// Forces Blish HUD to allow multiple instances.  Used internally to prevent race condition issues when Blish HUD is restarted.
        /// </summary>
        [
            Option(OPTION_RESTARTSKIPMUTEX),
            Help("Forces Blish HUD to allow multiple instances.  Used internally to prevent race condition issues when Blish HUD is restarted.")
        ]
        public bool RestartSkipMutex { get; private set; }

        #endregion

    }
}
