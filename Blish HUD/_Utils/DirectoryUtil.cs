using System;
using System.IO;

namespace Blish_HUD {

    /// <summary>
    /// Provides a way to get the root application save directory and to easily make new folders within it.
    /// </summary>
    public static class DirectoryUtil {

        private const string ADDON_DIR = @"Guild Wars 2\addons\blishhud";

        private const string SCREENS_DIR = @"Guild Wars 2\Screens";

        private const string MUSIC_DIR = @"Guild Wars 2\Music";

        private const string PROGRAMDATA_DIR = "Blish HUD";

        private const string CACHE_DIR = @"cache";

        /// <summary>
        /// The current root application save path used for saving settings, letting modules save data, etc.
        /// By default it is found in "Documents\Guild Wars 2\addons\blishhud."
        /// </summary>
        public static string BasePath { get; }

        /// <summary>
        /// The path used by the game client for saving screenshots made in the game (usually using the print screen key).
        /// By default it is found in "Documents\Guild Wars 2\Screens."
        /// </summary>
        public static string ScreensPath { get; }

        /// <summary>
        /// The path used by the game client for loading custom music playlists in a context-sensitive manner during gameplay.
        /// By default it is found in "Documents\Guild Wars 2\Music."
        /// </summary>
        public static string MusicPath { get; }

        /// <summary>
        /// The path used by Blish HUD to store non-user data related to the application.  This is kept in '%ProgramData%\Blish HUD' by default.
        /// This directory should likely not contain module data.
        /// </summary>
        public static string ProgramData { get; set; }

        /// <summary>
        /// The path used by Blish HUD to store shared cache data.  This is kept in \cache under <see cref="ProgramData"/>.
        /// </summary>
        public static string CachePath { get; }

        static DirectoryUtil() {
            // Prepare user documents directory
            // Check if Blish directory contains "Settings" folder
            // in that case override MyDocuments location as default for portability
            // --settings cli argument still has priority
            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Settings"))) {
                BasePath = ApplicationSettings.Instance.UserSettingsPath
                        ?? Path.Combine(Directory.GetCurrentDirectory(), "Settings");
            } else {
                BasePath = ApplicationSettings.Instance.UserSettingsPath
                        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                                   Environment.SpecialFolderOption.DoNotVerify),
                                        ADDON_DIR);
            }

            // Directories under ProgramData
            CreateDir(ProgramData = ApplicationSettings.Instance.ProgramDataPath
                                 ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData,
                                                                           Environment.SpecialFolderOption.DoNotVerify), PROGRAMDATA_DIR));

            CreateDir(CachePath = Path.Combine(ProgramData, CACHE_DIR));

            // Directories under documents
            CreateDir(BasePath);

            CreateDir(ScreensPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                                           Environment.SpecialFolderOption.DoNotVerify), SCREENS_DIR));

            CreateDir(MusicPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                                         Environment.SpecialFolderOption.DoNotVerify), MUSIC_DIR));
        }

        private static string CreateDir(string dirPath) {
            try {
                return Directory.CreateDirectory(dirPath).FullName;
            } catch (UnauthorizedAccessException) {
                Debug.Contingency.NotifyFileSaveAccessDenied(dirPath, string.Empty);
            }

            return null;
        }

        public static string RegisterDirectory(string directory) {
            return CreateDir(Path.Combine(BasePath, directory));
        }

        public static string RegisterDirectory(string basePath, string directory) {
            return CreateDir(Path.Combine(basePath, directory));
        }

    }

}
