using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Blish_HUD.Debug {
    internal static class ContingencyChecks {

        #region Launch Checks

        private static Task<string[]> _enabledNvidiaSettings;

        internal static void RunAll() {
            // Runs in the background so we don't delay Blish HUD's startup.
            _enabledNvidiaSettings = Task.Run(GetEnabledNvidiaSettings);

            CheckArcDps11Injected();
            CheckMinTls12();
            CheckControlledFolderAccessBlocking();
        }

        internal static void ReportDeferredChecks() {
            string[] enabledNvidiaSettings = _enabledNvidiaSettings?.Result;

            if (enabledNvidiaSettings?.Length > 0) {
                Contingency.NotifyNvidiaSettings(string.Join(Environment.NewLine, enabledNvidiaSettings.Select(name => string.Format(Strings.GameServices.Debug.ContingencyMessages.NvidiaSettings_Error, name, "Off"))));
            }
        }

        /// <summary>
        /// Typically occurs when ArcDps is placed in the same directory as Blish HUD
        /// and causes Blish HUD to crash almost immediately due to an access violation.
        /// </summary>
        private static void CheckArcDps11Injected() {
            // TODO: Get SetDllDirectory("") working so that we can protect ourselves from this!
            if (File.Exists("d3d11.dll")) {
                Contingency.NotifyArcDpsSameDir();
            }
        }

        /// <summary>
        /// Typically occurs on Windows 7 where Tls 1.2 has not been configured as the default.
        /// </summary>
        private static void CheckMinTls12() {
            // https://link.blishhud.com/tls12issue
            if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12)) {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
        }

        /// <summary>
        /// Indicates if CFA (Windows Ransomeware protection) is enabled.
        /// This feature prevents us from initializing our log file or writing out our settings.
        /// </summary>
        private static void CheckControlledFolderAccessBlocking() {
            using (var cfaRoot = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access")) {
                if (cfaRoot == null) {
                    return;
                }

                if (cfaRoot.GetValue("EnableControlledFolderAccess", 0) as int? == 1) {
                    try {
                        string cfaTestFile = Path.Combine(DirectoryUtil.BasePath, ".cfa");

                        File.WriteAllText(cfaTestFile, "cfa");

                        if (File.Exists(cfaTestFile) && File.ReadAllText(cfaTestFile) == "cfa") {
                            File.Delete(cfaTestFile);
                        }
                    } catch (Exception) {
                        // The chances that this isn't CFA are pretty slim.
                        Contingency.NotifyCfaBlocking(DirectoryUtil.BasePath);
                    }
                }
            }
        }

        // NVIDIA control panel settings that make Blish HUD render with an opaque background when set to 1 (on)
        private static readonly (uint Id, string Name)[] _forbiddenNvidiaSettings = {
            (0x1074C972, "Antialiasing - FXAA"),           // FXAA_ENABLE_ID
            (0x0098C1AC, "Multi-Frame Sampled AA (MFAA)"), // MAXWELL_B_SAMPLE_INTERLEAVE_ID
        };

        /// <summary>
        /// Returns the names of the forbidden NVIDIA settings that are on for Blish HUD.
        /// Runs off the main thread and never throws.
        /// </summary>
        private static string[] GetEnabledNvidiaSettings() {
            try {
                // Null right away on systems without an NVIDIA driver
                var values = NvidiaDrs.GetDwordSettings(Application.ExecutablePath, _forbiddenNvidiaSettings.Select(setting => setting.Id));

                return _forbiddenNvidiaSettings.Where(setting => values != null && values.TryGetValue(setting.Id, out uint value) && value == 1)
                                               .Select(setting => setting.Name)
                                               .ToArray();
            } catch (Exception) {
                return null; // Not a big deal if this fails
            }
        }

        #endregion

        #region Initiated Checks

        /// <summary>
        /// Checks to see if Guild Wars 2 is running in fullscreen mode along with DX9.
        /// Blish HUD can't run while the game is configured this way.
        /// </summary>
        internal static void CheckForFullscreenDx9Conflict() {
            if (GameService.GameIntegration.Gw2Instance.GraphicsApi == GameIntegration.Gw2Instance.Gw2GraphicsApi.DX9 
             && GameService.GameIntegration.GfxSettings.ScreenMode  == GameIntegration.GfxSettings.ScreenModeSetting.Fullscreen) {
                Contingency.NotifyConflictingFullscreenSettings();
            }
        }

        #endregion
    }
}
