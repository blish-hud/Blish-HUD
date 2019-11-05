using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Presenters;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Api.UI.Views {
    public class ManageApiKeyView : View<ManageApiKeyPresenter> {

        private Dropdown _keySelectionDropdown;
        private Label    _connectionLabel;
        private TextBox  _apiKeyTextBox;
        private Label    _apiKeyError;

        public Dropdown KeySelectionDropdown => _keySelectionDropdown;
        public Label    ConnectionLabel      => _connectionLabel;
        public TextBox  ApiKeyTextBox        => _apiKeyTextBox;
        public Label    ApiKeyError          => _apiKeyError;

        public ManageApiKeyView() {
            this.Presenter = new ManageApiKeyPresenter(this, GameService.Gw2Api);
        }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            return base.Load(progress);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            _keySelectionDropdown = new Dropdown() {
                Parent = buildPanel,
                Size   = new Point(200, 30)
            };

            _connectionLabel = new Label() {
                Parent              = buildPanel,
                Size                = new Point(buildPanel.Size.X, 30),
                ShowShadow          = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text                = "Not connected.",
                TextColor           = Color.IndianRed
            };

            _ = new Label() {
                Parent              = buildPanel,
                Size                = new Point(buildPanel.Size.X, 30),
                Location            = new Point(0,                 buildPanel.Size.Y / 2 - buildPanel.Size.Y / 4 - 15),
                ShowShadow          = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text                = "Insert your Guild Wars 2 API key here to unlock lots of cool features:"
            };

            _apiKeyTextBox = new TextBox() {
                Parent   = buildPanel,
                Size     = new Point(600,                         30),
                Location = new Point(buildPanel.Size.X / 2 - 300, buildPanel.Bottom),

                //PlaceholderText = keySelectionDropdown.SelectedItem != null ?
                //    foolSafeKeyRepository[keySelectionDropdown.SelectedItem] +
                //    Gw2ApiService.PLACEHOLDER_KEY.Substring(foolSafeKeyRepository.FirstOrDefault().Value.Length - 1)
                //    : Gw2ApiService.PLACEHOLDER_KEY
            };

            _apiKeyError = new Label() {
                Parent              = buildPanel,
                Size                = new Point(buildPanel.Size.X, 30),
                Location            = new Point(0,                 _apiKeyTextBox.Bottom + Control.ControlStandard.PanelOffset.Y),
                ShowShadow          = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor           = Color.Red,
                Text                = "Invalid API key! Try again.",
                Visible             = true
            };

            var apiKeyButton = new StandardButton() {
                Parent = buildPanel,
                Size = new Point(30, 30),
                Location = new Point(_apiKeyTextBox.Right, _apiKeyTextBox.Location.Y),
                Text = "",
                BackgroundColor = Color.IndianRed,
                //Visible = keySelectionDropdown.SelectedItem != null
            };
        }

        /// <inheritdoc />
        protected override void Unload() {
            base.Unload();
        }

    }
}
