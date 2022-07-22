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

        private const string CACHE_DIR = @"Blish HUD\cache";

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
        /// The path used by Blish HUD to store shared cache data.  This is kept in %ProgramData% by default.
        /// </summary>
        public static string CachePath { get; }

        static DirectoryUtil() {
            // Prepare user documents directory
            // Check if Blish directory contains "Settings" folder
            // in that case override MyDocuments location as default for portability
            // --settings cli argument has still priority
            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Settings"))) {
                BasePath = ApplicationSettings.Instance.UserSettingsPath
                        ?? Path.Combine(Directory.GetCurrentDirectory(), "Settings");
            } else {
                BasePath = ApplicationSettings.Instance.UserSettingsPath
                        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                                   Environment.SpecialFolderOption.DoNotVerify),
                                        ADDON_DIR);
            }

            CreateDir(BasePath);

            // Cache path is shared and not kept within the standard settings folder.
            CreateDir(CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData,
                                                                         Environment.SpecialFolderOption.DoNotVerify), CACHE_DIR));

            CreateDir(ScreensPath = ApplicationSettings.Instance.UserSettingsPath
                                 ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                                           Environment.SpecialFolderOption.DoNotVerify), SCREENS_DIR));

            CreateDir(MusicPath = ApplicationSettings.Instance.UserSettingsPath
                               ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
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
