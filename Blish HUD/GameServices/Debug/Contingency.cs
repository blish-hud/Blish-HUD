using Ookii.Dialogs.WinForms;
using System.Collections.Generic;
using System.Diagnostics;

namespace Blish_HUD.Debug {
    public static class Contingency {

        private const string DISCORD_JOIN_URL = "http://link.blishhud.com/discordhelp";

        private static readonly HashSet<string> _contingency = new HashSet<string>();

        private static void NotifyContingency(string key, string title, string description, string url) {
            if (_contingency.Contains(key)) {
                return;
            }

            _contingency.Add(key);

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

    }
}
