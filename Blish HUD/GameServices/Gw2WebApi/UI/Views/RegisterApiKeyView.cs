using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2WebApi.UI.Presenters;
using Gw2Sharp.WebApi.V2.Clients;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Gw2WebApi.UI.Views {
    public class RegisterApiKeyView : View<RegisterApiKeyPresenter> {

        private enum ApiTokenStatusType {
            Loading,
            Neutral,
            Failed,
            Partial,
            Perfect
        }

        private static readonly Logger Logger = Logger.GetLogger<RegisterApiKeyView>();

        private const int MAX_KEYNAME_LENGTH = 20;

        private readonly Dictionary<ApiTokenStatusType, Texture2D> _tokenStatusTextures = new Dictionary<ApiTokenStatusType, Texture2D>() {
            {ApiTokenStatusType.Neutral, GameService.Content.GetTexture(@"common\154983")},
            {ApiTokenStatusType.Failed, GameService.Content.GetTexture(@"common\154982")},
            {ApiTokenStatusType.Partial, GameService.Content.GetTexture(@"common\154981")},
            {ApiTokenStatusType.Perfect, GameService.Content.GetTexture(@"common\154979")}
        };

        public string ApiKey {
            get => _apiKeyTextBox.Text.Trim();
            set => _apiKeyTextBox.Text = value;
        }

        private TokenInfo _tokenInfo;

        public TokenInfo TokenInfo {
            get => _tokenInfo;
            set => _tokenInfo = value;
        }

        private TextBox        _apiKeyTextBox;
        private Image          _tokenStatusImg;
        private LoadingSpinner _loadingSpinner;
        private Label          _tokenStatusLbl;
        private StandardButton _registerKeyBttn;
        private FlowPanel      _tokensList;

        private CancellationTokenSource _tokenTestCanceller;

        private readonly Action<string> _debounceWrapper;

        public RegisterApiKeyView() {
            this.Presenter = new RegisterApiKeyPresenter(this, GameService.Gw2WebApi);

            Action<string> a = CheckToken;
            _debounceWrapper = a.Debounce();
        }

        protected override void Build(Panel buildPanel) {
            var registerLbl = new Label() {
                Text           = "Register API Key",
                Font           = GameService.Content.DefaultFont32,
                StrokeText     = true,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Location       = new Point(25, 25),
                Parent         = buildPanel
            };

            _apiKeyTextBox = new TextBox() {
                Location = new Point(registerLbl.Left, registerLbl.Bottom + 10),
                Font     = GameService.Content.DefaultFont16,
                Width    = buildPanel.Width - 50,
                Height   = 43,
                Parent   = buildPanel
            };

            _apiKeyTextBox.TextChanged += ApiKeyTextBoxOnTextChanged;

            _registerKeyBttn = new StandardButton() {
                Text   = "Register",
                Width  = 96,
                Right  = _apiKeyTextBox.Right,
                Top    = _apiKeyTextBox.Bottom + 5,
                Parent = buildPanel
            };

            var clearKeyBttn = new StandardButton() {
                Text   = "Clear",
                Width  = 96,
                Right  = _registerKeyBttn.Left - 10,
                Top    = _registerKeyBttn.Top,
                Parent = buildPanel
            };

            _tokenStatusImg = new Image() {
                Size     = new Point(32,                    32),
                Location = new Point(_apiKeyTextBox.Left, _apiKeyTextBox.Bottom + 5),
                Parent   = buildPanel
            };

            _loadingSpinner = new LoadingSpinner() {
                Size     = _tokenStatusImg.Size,
                Location = _tokenStatusImg.Location,
                Visible  = false,
                Parent   = buildPanel
            };

            _tokenStatusLbl = new Label() {
                Font          = GameService.Content.DefaultFont16,
                AutoSizeWidth = true,
                Location      = new Point(_tokenStatusImg.Right + 5, _tokenStatusImg.Top),
                Height        = _tokenStatusImg.Height,
                Parent        = buildPanel
            };

            _tokensList = new FlowPanel() {
                Location             = new Point(0, _tokenStatusImg.Bottom + 25),
                Size                 = new Point(buildPanel.Width, buildPanel.Height - _tokenStatusImg.Bottom - 25),
                ControlPadding       = new Vector2(10, 10),
                PadLeftBeforeControl = true,
                PadTopBeforeControl  = true,
                CanScroll            = true,
                Parent               = buildPanel
            };

            clearKeyBttn.Click += delegate { ClearApiKey(); };

            _registerKeyBttn.Click += delegate {
                GameService.Gw2WebApi.RegisterKey("New key", this.ApiKey);
                
                AddApiKey(this.ApiKey);

                ClearApiKey();
            };

            foreach (var key in GameService.Gw2WebApi.GetKeys()) {
                AddApiKey(key);
            }

            SetTokenStatus("", ApiTokenStatusType.Neutral);
        }

        private void AddApiKey(string apiKey) {
            var nPanel = new ViewContainer() {
                Size     = new Point(354, 100),
                Parent   = _tokensList,
                ShowTint = true
            };

            nPanel.Show(new ApiTokenView(apiKey));
        }

        private CancellationToken GetToken() {
            _tokenTestCanceller?.Cancel();
            _tokenTestCanceller = new CancellationTokenSource();

            return new CancellationToken();
        }

        private int _delayIndex = 0;

        private void ApiKeyTextBoxOnTextChanged(object sender, EventArgs e) {
            SetTokenStatus("", ApiTokenStatusType.Loading);
            _debounceWrapper(this.ApiKey);
        }

        private void CheckToken(string apiKey) {
            if (apiKey.Length > 10) {
                try {
                    GameService.Gw2WebApi
                               .GetConnection(apiKey)
                               .Client.V2.TokenInfo
                               .GetAsync(GetToken())
                               .ContinueWith(UpdateTokenDetails);
                } catch (FormatException formatException) {
                    SetTokenStatus("Invalid token", ApiTokenStatusType.Failed);
                }
            } else {
                SetTokenStatus("", ApiTokenStatusType.Neutral);
            }
        }

        private void UpdateTokenDetails(Task<TokenInfo> tokenTask) {
            if (tokenTask.IsCanceled) return;

            if (tokenTask.Exception != null) {
                SetTokenStatus("Invalid token", ApiTokenStatusType.Failed);
                Logger.Warn(tokenTask.Exception, "Checking token failed.");
                return;
            }

            var allPermissions = Enum.GetValues(typeof(TokenPermission));

            for (int i = 1; i < allPermissions.Length; i++) {
                if (!tokenTask.Result.Permissions.List.Contains(new ApiEnum<TokenPermission>((TokenPermission)allPermissions.GetValue(i)))) {
                    SetTokenStatus($"{GetTokenName(tokenTask.Result.Name)} - Token missing permissions", ApiTokenStatusType.Partial);
                    return;
                }
            }

            SetTokenStatus($"{GetTokenName(tokenTask.Result.Name)} - Valid token", ApiTokenStatusType.Perfect);
        }

        private string GetTokenName(string tokenName) {
            if (string.IsNullOrWhiteSpace(tokenName)) {
                return "(no name)";
            }

            return tokenName.Length <= MAX_KEYNAME_LENGTH ? tokenName : tokenName.Substring(0, MAX_KEYNAME_LENGTH - 3) + "...";
        }

        private void SetTokenStatus(string tokenStatusDescription, ApiTokenStatusType tokenStatusType) {
            if (tokenStatusType == ApiTokenStatusType.Loading) {
                _tokenStatusLbl.Text = "";
                _tokenStatusImg.Hide();
                _loadingSpinner.Show();
                return;
            }

            _loadingSpinner.Hide();

            _tokenStatusLbl.Text    = tokenStatusDescription;
            _tokenStatusImg.Texture = _tokenStatusTextures[tokenStatusType];

            _registerKeyBttn.Enabled = tokenStatusType == ApiTokenStatusType.Partial
                                    || tokenStatusType == ApiTokenStatusType.Perfect;

            _tokenStatusImg.Show();
        }

        public void ClearApiKey() {
            _apiKeyTextBox.Text = "";
        }

    }
}
