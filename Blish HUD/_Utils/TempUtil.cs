using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD.Settings;

namespace Blish_HUD {
    public static class TempUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(TempUtil));

        private const string FILESPENDINGDELETION_SETTINGS = "FilesPendingDeletion";

        private static SettingEntry<List<string>> _filesPendingDeleting;

        private static SettingEntry<List<string>> FilesPendingDeletion => _filesPendingDeleting ??= GameService.Settings.RegisterRootSettingCollection(nameof(TempUtil)).DefineSetting(FILESPENDINGDELETION_SETTINGS, new List<string>());

        public static void VoidFilePendingDeletion(string path) {
            FilesPendingDeletion.Value.Remove(path);
        }

        public static void EnqueueFileForDeletion(string path) {
            if (!File.Exists(path)) return;

            Logger.Info($"File '{path}' enqueued for deletion.");

            FilesPendingDeletion.Value.Remove(path); // Prevent duplicates
            FilesPendingDeletion.Value.Add(path);
            GameService.Settings.Save();
        }

        internal static void HandleInternal() {
            foreach (string file in FilesPendingDeletion.Value) {
                if (!File.Exists(file)) continue;

                try {
                    File.Delete(file);
                } catch (Exception ex) {
                    Logger.Warn(ex, $"Failed to delete file '{file}' pending deletion.");
                }

                Logger.Info($"Deleted file '{file}'.");
            }

            FilesPendingDeletion.Value.Clear();
            GameService.Settings.Save();
        }

    }
}
