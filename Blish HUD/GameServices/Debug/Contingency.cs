using System;
using Ookii.Dialogs.WinForms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blish_HUD.Debug {
    public static class Contingency {

        private const string DISCORD_JOIN_URL = "http://link.blishhud.com/discordhelp";

        private static readonly HashSet<string> _contingency = new HashSet<string>();

        private static void NotifyContingency(string key, string title, string description, string url, params (TaskDialogButton Button, Func<Task> OnClick)[] extraActions) {
            if (_contingency.Contains(key)) {
                return;
            }

            _contingency.Add(key);

            // Disable hooks and hide the UI, just in case.
            GameService.Input?.DisableHooks();

            if (BlishHud.Instance.Form.InvokeRequired) {
                BlishHud.Instance.Form.Invoke((MethodInvoker)(() => BlishHud.Instance.Form.Hide()));
            } else {
                BlishHud.Instance.Form.Hide();
            }

            var notifDiag = new TaskDialog() {
                WindowTitle      = title,
                MainIcon         = TaskDialogIcon.Warning,
                MainInstruction  = description,
                FooterIcon       = TaskDialogIcon.Information,
                EnableHyperlinks = true,
                Footer           = string.Format(Strings.GameServices.Debug.ContingencyMessages.GenericUrl_Footer, url, DISCORD_JOIN_URL),
            };

            notifDiag.HyperlinkClicked += NotifDiag_HyperlinkClicked;

            notifDiag.Buttons.Add(new TaskDialogButton(ButtonType.Ok));

            foreach (var actions in extraActions) {
                notifDiag.Buttons.Add(actions.Button);
            }

            notifDiag.ButtonClicked += async (sender, e) => {
                foreach (var action in extraActions) {
                    if (e.Item == action.Button) {
                        await action.OnClick.Invoke();
                    }
                }
            };

            notifDiag.ShowDialog();
        }

        private static void NotifDiag_HyperlinkClicked(object sender, HyperlinkClickedEventArgs e) {
            Process.Start(e.Href);
        }

        internal static void NotifyWin32AccessDenied() {
            NotifyContingency(nameof(NotifyWin32AccessDenied),
                              Strings.GameServices.Debug.ContingencyMessages.Win32AccessDenied_Title,
                              Strings.GameServices.Debug.ContingencyMessages.Win32AccessDenied_Description,
                              "http://link.blishhud.com/win32accessdenied");
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
            NotifyContingency(nameof(NotifyCoreUpdateFailed),
                              Strings.GameServices.Debug.ContingencyMessages.CoreUpdateFailed_Title,
                              string.Format(Strings.GameServices.Debug.ContingencyMessages.CoreUpdateFailed_Description, version, failureException.Message),
                              "https://link.blishhud.com/coreupdatefailed");
        }

    }
}
