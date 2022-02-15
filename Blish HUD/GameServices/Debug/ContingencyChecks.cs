using System;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace Blish_HUD.Debug {
    internal static class ContingencyChecks {

        public static void RunAll() {
            CheckArcDps11Injected();
            CheckMinTls12();
            CheckControlledFolderAccessBlocking();
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

    }
}
