using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Blish_HUD.Debug {
    public static class Contingency {

        private static readonly Logger Logger = Logger.GetLogger(typeof(Contingency));

        private static readonly HashSet<string> _contingency = new HashSet<string>();

        private static void NotifyContingency(string key, string title, string description, string url, params ContingencyPopup.PopupButton[] extraActions) {
            if (_contingency.Contains(key)) {
                return;
            }

            _contingency.Add(key);

            Logger.Warn($"Contingency '{key}' was triggered!");

            if (BlishHud.Instance != null) {
                if (BlishHud.Instance.Form.InvokeRequired) {
                    BlishHud.Instance.Form.Invoke(new Action(() => BlishHud.Instance.Form.Hide()));
                } else {
                    BlishHud.Instance.Form.Hide();
                }
            }

            var notifDiag = new ContingencyPopup(title, description, url, extraActions);

            notifDiag.ShowDialog();
        }

        private static void OpenNvidaControlPanel() {
            try {
                Process.Start(Environment.ExpandEnvironmentVariables("%programfiles%\\NVIDIA Corporation\\Control Panel Client\\nvcplui.exe"));
            } catch (Win32Exception) {
                Process.Start("explorer.exe", "shell:AppsFolder\\NVIDIACorp.NVIDIAControlPanel_56jybvy8sckqj!NVIDIACorp.NVIDIAControlPanel");
            }
        }

        internal static void NotifyWin32AccessDenied() {
            NotifyContingency(nameof(NotifyWin32AccessDenied),
                              Strings.GameServices.Debug.ContingencyMessages.Win32AccessDenied_Title,
                              Strings.GameServices.Debug.ContingencyMessages.Win32AccessDenied_Description,
                              "https://link.blishhud.com/win32accessdenied");
        }

        internal static void NotifyArcDpsSameDir() {
            NotifyContingency(nameof(NotifyArcDpsSameDir),
                              Strings.GameServices.Debug.ContingencyMessages.ArcDpsSameDir_Title,
                              Strings.GameServices.Debug.ContingencyMessages.ArcDpsSameDir_Description,
                              "https://link.blishhud.com/arcdpssamedir");
        }

        internal static void NotifyMissingRef() {
            NotifyContingency(nameof(NotifyMissingRef),
                              Strings.GameServices.Debug.ContingencyMessages.MissingRef_Title,
                              Strings.GameServices.Debug.ContingencyMessages.MissingRef_Description,
                              "https://link.blishhud.com/missingref");
        }

        internal static void NotifyCfaBlocking(string path) {
            NotifyContingency(nameof(NotifyCfaBlocking),
                              Strings.GameServices.Debug.ContingencyMessages.CfaBlocking_Title,
                              string.Format(Strings.GameServices.Debug.ContingencyMessages.CfaBlocking_Description, path),
                              "https://link.blishhud.com/cfablocking");
        }

        internal static void NotifyNvidiaSettings(string description) {
            NotifyContingency(nameof(NotifyNvidiaSettings),
                              Strings.GameServices.Debug.ContingencyMessages.NvidiaSettings_Title,
                              string.Format(Strings.GameServices.Debug.ContingencyMessages.NvidiaSettings_Description, description),
                              "https://link.blishhud.com/nvidiasettings",
                              new ContingencyPopup.PopupButton(Strings.GameServices.Debug.ContingencyMessages.NvidiaSettings_OpenControlPanelAction, OpenNvidaControlPanel));
        }

        internal static void NotifyConflictingFullscreenSettings() {
            NotifyContingency(nameof(NotifyConflictingFullscreenSettings),
                              Strings.GameServices.Debug.ContingencyMessages.ConflictingFullscreenSettings_Title,
                              Strings.GameServices.Debug.ContingencyMessages.ConflictingFullscreenSettings_Description,
                              "https://link.blishhud.com/fullscreenmode");
        }

        public static void NotifyFileSaveAccessDenied(string path, string actionDescription, bool promptPortableMode = false) {
            // TODO: If promptPortabelMode is true, add a new button to the diag that allows the user to enable portable mode as a work around.
            NotifyContingency(nameof(NotifyFileSaveAccessDenied),
                              Strings.GameServices.Debug.ContingencyMessages.FileSaveAccessDenied_Title,
                              string.Format(Strings.GameServices.Debug.ContingencyMessages.FileSaveAccessDenied_Description, path, actionDescription),
                              "https://link.blishhud.com/filesaveaccessdenied");
        }

        public static void NotifyHttpAccessDenied(string actionDescription) {
            NotifyContingency(nameof(NotifyHttpAccessDenied),
                              Strings.GameServices.Debug.ContingencyMessages.HttpAccessDenied_Title,
                              string.Format(Strings.GameServices.Debug.ContingencyMessages.HttpAccessDenied_Description, actionDescription),
                              "https://link.blishhud.com/httpaccessdenied");
        }

        public static void NotifyCoreUpdateFailed(SemVer.Version version, Exception failureException) {
            NotifyCoreUpdateFailed(version, failureException.Message);
        }

        public static void NotifyCoreUpdateFailed(SemVer.Version version, string message) {
            NotifyContingency(nameof(NotifyCoreUpdateFailed),
                              Strings.GameServices.Debug.ContingencyMessages.CoreUpdateFailed_Title,
                              string.Format(Strings.GameServices.Debug.ContingencyMessages.CoreUpdateFailed_Description, version, message),
                              "https://link.blishhud.com/coreupdatefailed");
        }

    }
}
