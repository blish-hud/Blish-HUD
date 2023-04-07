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

        public static ApplicationSettings Instance => _instance;

        public bool CliExitEarly => this.UserFacingExceptionThrown || this.HelpInvoked;

        public ApplicationSettings() : base("Blish HUD") {
            _instance ??= this;

            InitDebug();
        }

        public override void OnUserFacingException(UserFacingException e, string message) {
            MessageBox.Show("Invalid launch option(s) specified.  See --help for available options.", "Failed to launch Blish HUD", MessageBoxButtons.OK);
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
         * g, startgw2  - The start mode for Gw2 (0 = don't start, 1 = start gw2, 2 = start gw2 autologin).
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

        public const string OPTION_STARTGW2 = "startgw2";
        /// <summary>
        /// If we should launch Gw2 as part of Blish HUD launching.
        /// 0 = no, 1 = yes, and 2 = yes with autologin.
        /// </summary>
        [
            OptionParameter(OPTION_STARTGW2, 'g'),
            Help("Allows you to launch Guild Wars 2 with Blish HUD (0 = don't start, 1 = start gw2, 2 = start gw2 autologin).")
        ]
        public int StartGw2 { get; private set; }

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
            OptionParameter(OPTION_REFPATH, 'r'),
            Help("The path to the ref.dat file.")
        ]
        public string RefPath { get; private set; } = "ref.dat";

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

        public const string OPTION_MAINPROCESSID = "mainprocessid";
        /// <summary>
        /// The main Blish HUD process id. Used internally for subprocesses.
        /// </summary>
        [
            OptionParameter(OPTION_MAINPROCESSID),
            Help("The main Blish HUD process id. Used internally for subprocesses.")
        ]
        public int? MainProcessId { get; private set; }

        #endregion

    }
}
