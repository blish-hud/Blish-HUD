using System.Diagnostics;
using EntryPoint;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Blish_HUD {
    [Help("Optional launch arguments that modify overlay behavior.")]
    public class ApplicationSettings : BaseCliArguments {

        private static ApplicationSettings _instance;

        internal static ApplicationSettings Instance => _instance;
        
        public ApplicationSettings() : base("Blish HUD") {
            _instance = this;

            InitDebug();
        }

        [Conditional("DEBUG")]
        private void InitDebug() {
            this.DebugEnabled = true;
        }

        #region Game Integration

        [
            OptionParameter("process", 'p'),
            Help("The name of the process to overlay (without '.exe').")
        ]
        public string ProcessName { get; private set; }

        [
            OptionParameter("window", 'w'),
            Help("The name of the window to overlay.")
        ]
        public string WindowName { get; private set; }

        [
            OptionParameter("mumble", 'm'),
            Help("The MumbleLink map name to be used.")
        ]
        public string MumbleMapName { get; private set; }

        #endregion

        #region Utility

        [
            OptionParameter("settings", 's'),
            Help("The path where Blish HUD will save settings and other files.")
        ]
        public string UserSettingsPath { get; private set; }

        [
            OptionParameter("ref", 'r'),
            Help("The path to the ref.dat file.")
        ]
        public string RefPath { get; private set; }

        [
            OptionParameter("maxfps", 'f'),
            Help("The frame rate Blish HUD should target when rendering.")
        ]
        public double TargetFramerate { get; private set; } = 60d;

        [
            Option("unlockfps", 'F'),
            Help("Unlocks the frame limit allowing Blish HUD to render as fast as possible.  This will cause high CPU utilization.")
        ]
        public bool UnlockFps { get; private set; }

        #endregion

        #region Debug

        [
            Option("debug", 'd'),
            Help("Launches Blish HUD in debug mode.")
        ]
        public bool DebugEnabled { get; private set; }

        [
            OptionParameter("module", 'M'),
            Help("The path to a module (*.bhm) that will be force loaded when Blish HUD launches.")
        ]
        public string DebugModulePath { get; private set; }

        #endregion

    }
}
