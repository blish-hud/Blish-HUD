using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Blish_HUD.Controls;
using Blish_HUD.Gw2WebApi.UI.Presenters;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2WebApi.UI.Views {
    public class ApiTokenView : View<ApiTokenPresenter> {

        private Label _accountNameLbl;
        private Label _tokenKeyLbl;
        private Label _tokenNameLbl;

        private string[] _characters;
        private bool     _active;

        public string AccountName {
            get => _accountNameLbl.Text;
            set => _accountNameLbl.Text = value;
        }

        public string[] Characters {
            get => _characters;
            set => SetCharacters(value);
        }

        public string TokenName {
            get => _tokenNameLbl.Text;
            set => _tokenNameLbl.Text = value;
        }

        public bool Active {
            get => _active;
            set => SetActive(value);
        }
        
        public ApiTokenView(string token) {
            this.Presenter = new ApiTokenPresenter(this, token);
        }

        private void SetCharacters(string[] characters) {
            _characters                   = characters;
            _tokenKeyLbl.Text             = $"{_characters.Length} Characters";
            _tokenKeyLbl.BasicTooltipText = string.Join("\n", _characters);
        }

        private void SetActive(bool active) {
            _active                   = active;
            _accountNameLbl.TextColor = active ? Color.Green : Color.White;
        }

        protected override void Build(Panel buildPanel) {
            _accountNameLbl = new Label() {
                Text           = "Account Name...",
                Location       = new Point(10, 10),
                AutoSizeHeight = true,
                Width          = buildPanel.Width - 20,
                Parent         = buildPanel
            };

            _tokenKeyLbl = new Label() {
                Text           = "Token Key...",
                AutoSizeHeight = true,
                Width          = buildPanel.Width / 4,
                Left           = _accountNameLbl.Left,
                Bottom         = buildPanel.Height - 10,
                Parent         = buildPanel
            };

            _tokenNameLbl = new Label() {
                Text                = "Token Name...",
                HorizontalAlignment = HorizontalAlignment.Right,
                AutoSizeHeight      = true,
                Width               = (buildPanel.Width / 4) * 3 - 20,
                Left                = _tokenKeyLbl.Right   + 10,
                Bottom              = buildPanel.Height    - 10,
                Parent              = buildPanel
            };
        }

    }
}
