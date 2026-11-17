using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl.Http;
using Humanizer;

namespace Blish_HUD.Overlay.SelfUpdater {
    internal static class SelfUpdateUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(SelfUpdateUtil));

        private const string FILE_UNPACKZIP = "unpack.zip";
        private const string FILE_EXE       = "Blish HUD.exe";
        private const string FILE_EXEBACKUP = FILE_EXE + "__bak";

        private const int RESTART_DELAY       = 3;
        private const int SINGLEPROCESS_DELAY = 10;

        // Files no longer used by Blish HUD which can be removed.
        private static readonly string[] _obsoleteFiles = new[] {
            "Blish HUD.DebugHelper.exe", // Input helper is essentially embedded now.
            "Blish HUD.pdb"              // Symbols are now embedded in the EXE.
        };

        public static (bool UpdateRelevant, bool Succeeded) TryHandleUpdate() {
            string unpackPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILE_UNPACKZIP);

            if (!File.Exists(unpackPath)) {
                return (false, false);
            }

            // Try to make sure we're the only Blish HUD instance running.
            if (!TryWaitForProcessLocks()) {
                Debug.Contingency.NotifyCoreUpdateFailed(Program.OverlayVersion, Strings.GameServices.Debug.ContingencyMessages.CoreUpdateFailed_Description_Timeout);
                return (true, false);
            }

            try {
                HandleUpdate(unpackPath);
            } catch (UnauthorizedAccessException) {
                Debug.Contingency.NotifyFileSaveAccessDenied(unpackPath, Strings.GameServices.Debug.ContingencyMessages.FileSaveAccessDenied_Action_ToUpdate);
            } catch (Exception ex) {
                // Implement retry button.
                Debug.Contingency.NotifyCoreUpdateFailed(Program.OverlayVersion, ex);
                return (true, false);
            }

            return (true, true);
        }

        private static void HandleUpdate(string unpackPath) {
            string applicationDir = Path.GetDirectoryName(Application.ExecutablePath);

            // Remove any old files from old versions
            foreach (string fileName in _obsoleteFiles) {
                DeleteFile(Path.Combine(applicationDir, fileName));
            }

            using (var unpackStream = File.OpenRead(unpackPath))
            using (var unpacker = new ZipArchive(unpackStream)) {
                var rootDirs = unpacker.Entries
                    .Select(entry => entry.FullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                    .Where(parts => parts.Length > 1)
                    .Select(parts => parts[0])
                    .Distinct()
                    .ToList();
                var allFiles  = unpacker.Entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && !string.Equals(entry.Name, FILE_EXE)).ToList();
                var rootFiles = allFiles.Where(entry => !entry.FullName.Contains("/"));

                foreach (string dirName in rootDirs) {
                    DeleteDirectory(Path.Combine(applicationDir, dirName));
                }

                foreach (var file in rootFiles) {
                    DeleteFile(Path.Combine(applicationDir, file.FullName));
                }

                foreach (var file in allFiles) {
                    string filePath = Path.Combine(applicationDir, file.FullName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    WithRetry(() => file.ExtractToFile(filePath, true));
                }
            }

            DeleteFile(Path.Combine(applicationDir, FILE_EXEBACKUP));
            DeleteFile(unpackPath);
        }

        private static void DeleteFile(string filePath) {
            if (!File.Exists(filePath)) {
                return;
            }

            WithRetry(() => {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            });
        }

        private static void DeleteDirectory(string dirPath) {
            if (!Directory.Exists(dirPath)) {
                return;
            }

            var attributes = File.GetAttributes(dirPath);

            // Clear ReadOnly (and others) so the directory itself can be removed (OneDrive marks them read-only which was causing tons of updates to fail)
            File.SetAttributes(dirPath, FileAttributes.Directory);

            if (attributes.HasFlag(FileAttributes.ReparsePoint)) {
                // Removes symlinks/junctions without following them - OneDrive folders are reparse points too, but fail here and recurse below
                try {
                    Directory.Delete(dirPath, false);
                    return;
                } catch (IOException) { /* Not empty - fall through to a recursive delete */ }
            }

            foreach (string filePath in Directory.EnumerateFiles(dirPath)) {
                DeleteFile(filePath);
            }

            foreach (string subDirPath in Directory.EnumerateDirectories(dirPath)) {
                DeleteDirectory(subDirPath);
            }

            WithRetry(() => Directory.Delete(dirPath, false));
        }

        private static void WithRetry(Action action, int attempts = 5) {
            // Retry a few times on IO failures in case OneDrive or something is holding on too tight
            for (int attempt = 1; ; attempt++) {
                try {
                    action();
                    return;
                } catch (IOException ex) when (attempt < attempts) {
                    Logger.Warn(ex, $"Update file operation failed (attempt {attempt} of {attempts}).  Retrying...");
                    System.Threading.Thread.Sleep(250 * attempt);
                } catch (UnauthorizedAccessException ex) when (attempt < attempts) {
                    Logger.Warn(ex, $"Update file operation was denied (attempt {attempt} of {attempts}).  Retrying...");
                    System.Threading.Thread.Sleep(250 * attempt);
                }
            }
        }

        private static bool TryWaitForProcessLocks(int attempts = SINGLEPROCESS_DELAY) {
            string processName = Path.GetFileNameWithoutExtension(FILE_EXE);

            for (int attempt = 1; ; attempt++) {
                Process[] instances = Process.GetProcessesByName(processName);
                int       others    = instances.Length - 1;

                foreach (var instance in instances) {
                    instance.Dispose();
                }

                if (others <= 0) {
                    return true;
                }

                if (attempt >= attempts) {
                    Logger.Warn($"Gave up waiting on {others} other Blish HUD instance(s) to close.");
                    return false;
                }

                Logger.Warn($"Waiting on {others} other Blish HUD instance(s) to close (attempt {attempt} of {attempts}).  Retrying...");
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static async Task BeginUpdate(CoreVersionManifest coreVersionManifest, IProgress<string> progress = null) {
            // Download the archive
            Logger.Info($"Downloading version v{coreVersionManifest.Version} from {coreVersionManifest.Url}...");
            progress?.Report(string.Format(coreVersionManifest.IsPrerelease 
                                               ? Strings.GameServices.OverlayService.SelfUpdate_Progress_DownloadingPrereleaseArchive
                                               : Strings.GameServices.OverlayService.SelfUpdate_Progress_DownloadingReleaseArchive,
                                           coreVersionManifest.Version));
            string unpackDestination = await coreVersionManifest.Url.DownloadFileAsync(Path.GetDirectoryName(Application.ExecutablePath), FILE_UNPACKZIP);
            Logger.Info($"Finished downloading {unpackDestination}.");

            // Verify the checksum
            progress?.Report(Strings.GameServices.OverlayService.SelfUpdate_Progress_VerifyingChecksum);
            using var dataSha256  = System.Security.Cryptography.SHA256.Create();
            using var unpackFile  = File.OpenRead(unpackDestination);
            byte[]    rawChecksum = dataSha256.ComputeHash(unpackFile);
            string    checksum    = BitConverter.ToString(rawChecksum).Replace("-", string.Empty);

            if (!string.Equals(coreVersionManifest.Checksum, checksum, StringComparison.InvariantCultureIgnoreCase)) {
                // Checksum does not match!  Reverting back and notifying the user.
                Logger.Warn("Got {actualChecksum} instead of the expected {expectedChecksum} as the checksum!  Aborting!", checksum, coreVersionManifest.Checksum);

                unpackFile.Close();
                File.Delete(unpackDestination);

                throw new Exception("Update failed!\nDownload was invalid (checksum failed).\nNo changes were made.");
            } else {
                Logger.Info($"Found the expected checksum '{coreVersionManifest.Checksum}'.");

                // Extract out the EXE and then restart.
                progress?.Report("Extracting new executable...");

                string currentPath = Path.GetDirectoryName(Application.ExecutablePath);
                string currentName = Path.GetFileName(Application.ExecutablePath);

                string exeBackupPath = Path.Combine(currentPath, FILE_EXEBACKUP);

                if (File.Exists(exeBackupPath)) {
                    File.Delete(exeBackupPath);
                }

                File.Move(Path.Combine(currentPath, currentName), exeBackupPath);

                var unpacker = new ZipArchive(unpackFile);
                unpacker.Entries.First(entry => entry.Name == FILE_EXE).ExtractToFile(Path.Combine(currentPath, currentName));

                unpackFile.Close();

                if (progress != null) {
                    for (int i = RESTART_DELAY; i > 0; i--) {
                        progress.Report(string.Format(Strings.GameServices.OverlayService.SelfUpdate_Progress_RestartingIn, TimeSpan.FromSeconds(i).Humanize()));
                        await Task.Delay(1000);
                    }

                    progress.Report(Strings.GameServices.OverlayService.SelfUpdate_Progress_Restarting);
                }

                GameService.Overlay.Restart();
            }
        }

    }
}
