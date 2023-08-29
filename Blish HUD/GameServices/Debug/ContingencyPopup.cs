using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Blish_HUD.Debug {
    internal partial class ContingencyPopup : Form {

        internal class PopupButton {

            public string Text { get; }
            public Action OnClick { get; }

            public PopupButton(string text, Action onClick) {
                this.Text    = text;
                this.OnClick = onClick;
            }

        }

        private const string DISCORD_JOIN_URL = "https://link.blishhud.com/discordhelp";

        public string TroubleshootingUrl { get; }

        public ContingencyPopup() {
            InitializeComponent();
        }

        public ContingencyPopup(string title, string description, string troubleshootingUrl, IEnumerable<PopupButton> buttons = null) {
            InitializeComponent();

            this.Text                = title;
            this.LblDescription.Text = description;
            this.TroubleshootingUrl  = troubleshootingUrl;

            if (buttons != null) {
                foreach (var button in buttons) {
                    var bttn = new Button { Text = button.Text, Parent = PnlAction, AutoSize = true };
                    bttn.Click += (sender, e) => { button.OnClick(); };
                }
            }
        }

        private void lblDiscordChannel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            OpenWebpage(DISCORD_JOIN_URL);
        }

        private void LblTroubleshootingGuide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            OpenWebpage(this.TroubleshootingUrl);
        }

        private static void OpenWebpage(string url) {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private bool _clickAttempted = false;

        private void BttnOkay_Click(object sender, EventArgs e) {
            if (_clickAttempted) {
                // Make sure folks can close the window
                Process.GetCurrentProcess().Kill();
            } else {
                Close();
            }

            _clickAttempted = true;
        }

        private void LblDescription_Resize(object sender, EventArgs e) {
            this.Height = this.LblDescription.Bottom + this.LblDescription.Top + PnlAction.Height + PnlExtraInfo.Height + 32;
        }
    }
}
