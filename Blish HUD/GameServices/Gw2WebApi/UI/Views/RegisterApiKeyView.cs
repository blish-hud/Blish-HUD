using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2WebApi.UI.Presenters;
using Gw2Sharp.WebApi.Http;
using Gw2Sharp.WebApi.V2.Models;
using Humanizer;
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

        private const int MAX_KEYNAME_LENGTH = 14;
        private const int MIN_KEY_LENGTH     = 10;

        private readonly Dictionary<ApiTokenStatusType, Texture2D> _tokenStatusTextures = new Dictionary<ApiTokenStatusType, Texture2D>() {
            {ApiTokenStatusType.Neutral, GameService.Content.GetTexture(@"common/154983")},
            {ApiTokenStatusType.Failed,  GameService.Content.GetTexture(@"common/154982")},
            {ApiTokenStatusType.Partial, GameService.Content.GetTexture(@"common/154981")},
            {ApiTokenStatusType.Perfect, GameService.Content.GetTexture(@"common/154979")}
        };

        public string ApiKey {
            get => _apiKeyTextBox.Text.Trim();
            set => _apiKeyTextBox.Text = value;
        }

        private (TokenInfo TokenInfo, Account AccountInfo) _loadedDetails;

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

            _debounceWrapper = ((Action<string>)CheckToken).Debounce();
        }

        protected override void Build(Panel buildPanel) {
            var registerLbl = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.ManageApiKeys_Title,
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
                Text   = Strings.GameServices.Gw2ApiService.ManageApiKeys_Register,
                Width  = 96,
                Right  = _apiKeyTextBox.Right,
                Top    = _apiKeyTextBox.Bottom + 5,
                Parent = buildPanel
            };

            var clearKeyBttn = new StandardButton() {
                Text   = Strings.Common.Action_Clear,
                Width  = 96,
                Right  = _registerKeyBttn.Left - 10,
                Top    = _registerKeyBttn.Top,
                Parent = buildPanel
            };

            _tokenStatusImg = new Image() {
                Size     = new Point(32,                  32),
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
                Location             = new Point(_apiKeyTextBox.Left,  _tokenStatusImg.Bottom                     + 25),
                Size                 = new Point(_apiKeyTextBox.Width, buildPanel.Height - _tokenStatusImg.Bottom - 25),
                ControlPadding       = new Vector2(11, 11),
                PadLeftBeforeControl = true,
                PadTopBeforeControl  = true,
                CanScroll            = true,
                ShowBorder = true,
                Parent               = buildPanel
            };

            _tokensList.Width = 387 + 22 + 4;

            clearKeyBttn.Click += delegate { ClearApiKey(); };

            _registerKeyBttn.Click += delegate {
                GameService.Gw2WebApi.RegisterKey(_loadedDetails.AccountInfo.Name, this.ApiKey);

                ReloadApiKeys();

                ClearApiKey();
            };

            var openAnetApplicationsBttn = new StandardButton() {
                Text     = "Manage Applications",
                Icon     = GameService.Content.GetTexture("common/1441452"),
                Width    = 128,
                Location = new Point(_tokensList.Right + 10, _tokensList.Top),
                Parent   = buildPanel
            };

            ReloadApiKeys();

            SetTokenStatus(ApiTokenStatusType.Neutral);
        }

        private void ReloadApiKeys() {
            _tokensList.ClearChildren();

            foreach (var key in GameService.Gw2WebApi.GetKeys()) {
                var nPanel = new ViewContainer() {
                    Size     = new Point(387, 82),
                    ShowTint = true,
                    Parent   = _tokensList
                };

                nPanel.Show(new ApiTokenView(key));
            }
        }

        private CancellationToken GetToken() {
            _tokenTestCanceller?.Cancel();
            _tokenTestCanceller = new CancellationTokenSource();

            return new CancellationToken();
        }

        private void ApiKeyTextBoxOnTextChanged(object sender, EventArgs e) {
            SetTokenStatus(ApiTokenStatusType.Loading);
            _debounceWrapper(this.ApiKey);
        }

        private async void CheckToken(string apiKey) {
            if (apiKey.Length > MIN_KEY_LENGTH) {
                try {
                    var sharedConnection = GameService.Gw2WebApi.GetConnection(apiKey);

                    var tokenInfo = sharedConnection
                                   .Client.V2.TokenInfo
                                   .GetAsync(GetToken());

                    var accountInfo = sharedConnection
                                     .Client.V2.Account
                                     .GetAsync();

                    await UpdateTokenDetails(tokenInfo, accountInfo);
                } catch (FormatException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_InvalidToken);
                } catch (InvalidAccessTokenException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_InvalidToken);
                } catch (AuthorizationRequiredException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_AccountFailed);
                }
            } else {
                SetTokenStatus(ApiTokenStatusType.Neutral);
            }
        }

        private async Task UpdateTokenDetails(Task<TokenInfo> tokenTask, Task<Account> accountTask) {
            await Task.WhenAll(tokenTask, accountTask);

            if (tokenTask.IsCanceled || accountTask.IsCanceled) return;

            if (tokenTask.Exception != null) {
                SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_InvalidToken);
                Logger.Warn(tokenTask.Exception, "Checking token failed.");
                return;
            }

            if (accountTask.Exception != null) {
                SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_AccountFailed);
                Logger.Warn(accountTask.Exception, "Getting account information failed.");
                return;
            }

            _loadedDetails = (tokenTask.Result, accountTask.Result);

            var allPermissions = Enum.GetValues(typeof(TokenPermission));

            for (int i = 1; i < allPermissions.Length; i++) {
                if (!tokenTask.Result.Permissions.List.Contains(new ApiEnum<TokenPermission>((TokenPermission)allPermissions.GetValue(i)))) {
                    SetTokenStatus(ApiTokenStatusType.Partial, Strings.GameServices.Gw2ApiService.TokenStatus_PartialPermission);
                    return;
                }
            }

            SetTokenStatus(ApiTokenStatusType.Perfect,
                           string.Format(Strings.GameServices.Gw2ApiService.TokenStatus_ValidToken,
                                         accountTask.Result.Name,
                                         GetTokenName(tokenTask.Result.Name)));
        }

        private string GetTokenName(string tokenName) {
            if (string.IsNullOrWhiteSpace(tokenName)) {
                return Strings.GameServices.Gw2ApiService.Token_NoName;
            }

            return tokenName.Truncate(MAX_KEYNAME_LENGTH, "...");
        }

        private void SetTokenStatus(ApiTokenStatusType tokenStatusType, string tokenStatusDescription = "") {
            _tokenStatusLbl.Text = tokenStatusDescription;

            if (tokenStatusType == ApiTokenStatusType.Loading) {
                _tokenStatusImg.Hide();
                _loadingSpinner.Show();
                return;
            }

            _loadingSpinner.Hide();
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
