using System;
using EntryPoint;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Blish_HUD {
    [Help("Optional launch arguments that modify overlay behavior.")]
    public class ApplicationSettings : BaseCliArguments {

        private static ApplicationSettings _instance;

        internal static ApplicationSettings Instance => _instance;

        public ApplicationSettings() : base("Blish HUD") {
            _instance = this;
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

        #endregion

    }
}
