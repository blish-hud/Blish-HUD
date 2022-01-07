using System;
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

        private const int RESTART_DELAY = 3;

        public static (bool UpdateRelevant, bool Succeeded) TryHandleUpdate() {
            string unpackPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILE_UNPACKZIP);

            if (!File.Exists(unpackPath)) {
                return (false, false);
            }

            // Try to make sure parent process has closed so none of the
            // files are still locked - there are better ways to do this
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

            try {
                HandleUpdate(unpackPath);
            } catch (UnauthorizedAccessException) {
                Debug.Contingency.NotifyFileSaveAccessDenied(unpackPath, Strings.GameServices.Debug.ContingencyMessages.FileSaveAccessDenied_Action_ToUpdate);
            } catch (Exception ex) {
                Debug.Contingency.NotifyCoreUpdateFailed(Program.OverlayVersion, ex);
                return (true, false);
            }

            return (true, true);
        }

        private static void HandleUpdate(string unpackPath) {
            var unpackStream = File.OpenRead(unpackPath);
            var unpacker     = new ZipArchive(unpackStream);

            var applicationDir = Directory.GetCurrentDirectory();

            var rootDirs  = unpacker.Entries.Where(entry => string.IsNullOrWhiteSpace(entry.Name)  && entry.FullName.IndexOf('/') == entry.FullName.Length - 1);
            var allFiles  = unpacker.Entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && !string.Equals(entry.Name, FILE_EXE));
            var rootFiles = allFiles.Where(entry => !entry.FullName.Contains("/"));

            foreach (var dir in rootDirs) {
                string dirPath = Path.Combine(applicationDir, dir.FullName);

                if (Directory.Exists(dirPath)) {
                    Directory.Delete(dirPath, true);
                }
            }

            foreach (var file in rootFiles) {
                string filePath = Path.Combine(applicationDir, file.FullName);

                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                }
            }

            foreach (var file in allFiles) {
                string dir = Path.GetDirectoryName(file.FullName);

                Directory.CreateDirectory(Path.Combine(applicationDir, dir));
                file.ExtractToFile(Path.Combine(applicationDir,        file.FullName));
            }

            if (File.Exists(Path.Combine(applicationDir, FILE_EXEBACKUP))) {
                File.Delete(Path.Combine(applicationDir, FILE_EXEBACKUP));
            }

            unpacker.Dispose();
            unpackStream.Dispose();

            File.Delete(unpackPath);
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

                unpackFile.Dispose();
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

                unpackFile.Dispose();

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
