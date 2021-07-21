using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD.Controls;
using Blish_HUD.Overlay.SelfUpdater;
using Flurl.Http;

namespace Blish_HUD {
    internal static class SelfUpdateUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(SelfUpdateUtil));

        private const string FILE_UNPACKZIP = "unpack.zip";
        private const string FILE_EXE       = "Blish HUD.exe";
        private const string FILE_EXEBACKUP = FILE_EXE + "__bak";

        public static bool TryHandleUpdate() {
            string unpackPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILE_UNPACKZIP);
            
            if (!File.Exists(unpackPath)) {
                return true;
            }

            if (ApplicationSettings.Instance.ParentPid > 0) {
                // Wait up to 10 seconds for the previous Blish HUD instance to close.
                for (int i = 20; Process.GetProcesses().Any(x => x.Id == ApplicationSettings.Instance.ParentPid) && i > 0; i++) {
                    System.Threading.Thread.Sleep(500);
                }
            }

            try {
                HandleUpdate(unpackPath);
            } catch (Exception ex) {
                MessageBox.Show($"Failed to complete update!  Launch Blish HUD again to try to complete the update again.\r\n\r\n{ex.Message}", "", MessageBoxButtons.OK);
                return false;
            }

            return true;
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
                file.ExtractToFile(Path.Combine(applicationDir, file.FullName));
            }

            if (File.Exists(Path.Combine(applicationDir, FILE_EXEBACKUP))) {
                File.Delete(Path.Combine(applicationDir, FILE_EXEBACKUP));
            }

            unpacker.Dispose();
            unpackStream.Dispose();

            File.Delete(unpackPath);
        }

        public static async Task BeginUpdate(CoreVersionManifest coreVersionManifest) {
            Logger.Info($"Downloading version v{coreVersionManifest.Version}...");
            string unpackDestination = await coreVersionManifest.Url.DownloadFileAsync(Path.GetDirectoryName(Application.ExecutablePath), FILE_UNPACKZIP);
            Logger.Info($"Finished downloading {unpackDestination}.");

            using var dataSha256  = System.Security.Cryptography.SHA256.Create();
            using var unpackFile  = File.OpenRead(unpackDestination);
            byte[]    rawChecksum = dataSha256.ComputeHash(unpackFile);
            string    checksum    = BitConverter.ToString(rawChecksum).Replace("-", string.Empty);

            if (!string.Equals(coreVersionManifest.Checksum, checksum, StringComparison.InvariantCultureIgnoreCase)) {
                ScreenNotification.ShowNotification("Update failed!  Download was invalid (checksum failed).\r\nBlish HUD will restart.  No changes were made.", ScreenNotification.NotificationType.Error, null, 8);
                unpackFile.Dispose();
                File.Delete(unpackDestination);

                await Task.Delay(TimeSpan.FromSeconds(8));

                GameService.Overlay.Restart();
                return;
            } else {
                Logger.Info($"Found the expected checksum '{coreVersionManifest.Checksum}'.");

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

                GameService.Overlay.Restart($"--{ApplicationSettings.OPTION_PARENTPID} {Process.GetCurrentProcess().Id}");
            }
        }

    }
}
