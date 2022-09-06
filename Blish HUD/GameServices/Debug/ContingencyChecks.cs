using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using nspector.Common;
using nspector.Common.CustomSettings;
using nspector.Common.Meta;
using nspector.Native.NvApi.DriverSettings;

namespace Blish_HUD.Debug {
    internal static class ContingencyChecks {

        public static void RunAll() {
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

        private static void CheckNvidiaControlPanelSettings() {
            try {
                CustomSettingNames customSettingNames =  CustomSettingNames.FactoryLoadFromString(nspector.Properties.Resources.CustomSettingNames);
                CustomSettingNames referenceSettingNames = CustomSettingNames.FactoryLoadFromString(nspector.Properties.Resources.ReferenceSettingNames);

                DrsSettingsMetaService metaService = new DrsSettingsMetaService(customSettingNames, referenceSettingNames);
                DrsDecrypterService decrypterService = new DrsDecrypterService(metaService);
                DrsScannerService scannerService = new DrsScannerService(metaService, decrypterService);
                DrsSettingsService settingService = new DrsSettingsService(metaService, decrypterService);

                // this might be nicer as a resource or a config file?
                Dictionary<ESetting, HashSet<uint>> forbiddenValues = new Dictionary<ESetting, HashSet<uint>>() {
                    [ESetting.MAXWELL_B_SAMPLE_INTERLEAVE_ID] = new HashSet<uint>() { 1 }
                };

                // stored using forward slashes for some reason
                string exePath = Application.ExecutablePath.Replace('\\', '/');
                string blishProfileName = scannerService.FindProfilesUsingApplication(exePath);

                List<string> errors = new List<string>();
                foreach (KeyValuePair<ESetting, HashSet<uint>> pair in forbiddenValues) {
                    SettingMeta settingMeta = metaService.GetSettingMeta((uint)pair.Key);
                    uint value = settingService.GetDwordValueFromProfile(blishProfileName, (uint)pair.Key);

                    if (pair.Value.Contains(value)) {
                        SettingValue<uint> settingValue = settingMeta.DwordValues.FirstOrDefault(val => val.Value == value);
                        string val = settingValue?.ValueName != null ? settingValue.ValueName : value.ToString();

                        errors.Add($"'{settingMeta.SettingName}' = '{val}'");
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
    }
}
