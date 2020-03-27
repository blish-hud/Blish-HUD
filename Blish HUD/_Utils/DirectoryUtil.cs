using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {

    /// <summary>
    /// Provides a way to get the root application save directory and to easily make new folders within it.
    /// </summary>
    public static class DirectoryUtil {

        private const string ADDON_DIR = @"Guild Wars 2\addons\blishhud";

        /// <summary>
        /// The current root application save path used for saving settings, letting modules save data, etc.
        /// By default it is found in "Documents\Guild Wars 2\addons\blishhud."
        /// </summary>
        public static string BasePath { get; }

        static DirectoryUtil() {
            // Prepare user documents directory
            BasePath = ApplicationSettings.Instance.UserSettingsPath
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments,
                                                               Environment.SpecialFolderOption.DoNotVerify),
                                     ADDON_DIR);

            Directory.CreateDirectory(BasePath);
        }

        public static string RegisterDirectory(string directory) => Directory.CreateDirectory(Path.Combine(BasePath, directory)).FullName;

    }

}
