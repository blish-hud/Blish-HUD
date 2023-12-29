using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Microsoft.Win32;
using nspector.Common;
using nspector.Common.CustomSettings;
using nspector.Common.Meta;
using nspector.Native.NvApi.DriverSettings;

namespace Blish_HUD.Debug {
    internal static class ContingencyChecks {

        #region Launch Checks

        internal static void RunAll() {
            CheckArcDps11Injected();
            CheckMinTls12();
            CheckControlledFolderAccessBlocking();
            CheckNvidiaControlPanelSettings();
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

        /// <summary>
        /// Checks to ensure that non-default Nvidia control panel settings haven't been set for Blish HUD.
        /// For specific settings, this can cause Blish HUD to render with an opaque background.
        /// </summary>
        private static void CheckNvidiaControlPanelSettings() {
            try {
                var customSettingNames    = CustomSettingNames.FactoryLoadFromString(nspector.Properties.Resources.CustomSettingNames);
                var referenceSettingNames = CustomSettingNames.FactoryLoadFromString(nspector.Properties.Resources.ReferenceSettingNames);

                var metaService      = new DrsSettingsMetaService(customSettingNames, referenceSettingNames);
                var decrypterService = new DrsDecrypterService(metaService);
                var scannerService   = new DrsScannerService(metaService, decrypterService);
                var settingService   = new DrsSettingsService(metaService, decrypterService);

                var forbiddenValues = new Dictionary<ESetting, HashSet<uint>>() {
                    [ESetting.FXAA_ENABLE_ID] = new HashSet<uint>() { 1 },
                    [ESetting.MAXWELL_B_SAMPLE_INTERLEAVE_ID] = new HashSet<uint>() { 1 }
                };

                // stored using forward slashes for some reason
                string exePath = Application.ExecutablePath.Replace('\\', '/');
                string blishProfileName = scannerService.FindProfilesUsingApplication(exePath);

                var errors = new List<string>();
                foreach (KeyValuePair<ESetting, HashSet<uint>> pair in forbiddenValues) {
                    SettingMeta settingMeta = metaService.GetSettingMeta((uint)pair.Key);
                    uint value = settingService.GetDwordValueFromProfile(blishProfileName, (uint)pair.Key);

                    if (pair.Value.Contains(value)) {
                        SettingValue<uint> settingValue = settingMeta.DwordValues.FirstOrDefault(val => val.Value == value);
                        string val = settingValue?.ValueName ?? value.ToString();

                        errors.Add(string.Format(Strings.GameServices.Debug.ContingencyMessages.NvidiaSettings_Error, settingMeta.SettingName, val));
                    }
                }

                if (errors.Any()) {
                    Contingency.NotifyNvidiaSettings(string.Join(Environment.NewLine, errors));
                }
            } catch (Exception) {
                 // we don't really care if we error here - usually means a non-nvidia system,
                 // in which case the check is useless anyway.
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
