using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2WebApi.UI.Presenters;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Gw2WebApi.UI.Views {
    public class RegisterApiKeyView : View {

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

        private readonly TokenPermission[] _minimumTokenPermissions = {
            TokenPermission.Account,
            TokenPermission.Characters
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

        private readonly Action<string> _tokenCheckDebounceWrapper;

        public RegisterApiKeyView() {
            _tokenCheckDebounceWrapper = ((Action<string>)CheckToken).Debounce();
        }

        protected override void Build(Container buildPanel) {
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

            _tokensList = new FlowPanel {
                Location            = new Point(_apiKeyTextBox.Left, _tokenStatusImg.Bottom                     + 25),
                Size                = new Point(380,                 buildPanel.Height - _tokenStatusImg.Bottom - 25),
                OuterControlPadding = new Vector2(11, 11),
                CanScroll           = true,
                ShowBorder          = true,
                Parent              = buildPanel
            };


            clearKeyBttn.Click += delegate { ClearApiKey(); };

            _registerKeyBttn.Click += delegate {
                GameService.Gw2WebApi.RegisterKey(_loadedDetails.AccountInfo.Name, this.ApiKey);

                ReloadApiKeys();

                ClearApiKey();
            };

            var instructions = new Label() {
                Text           = Strings.Common.Instructions,
                Location       = new Point(_tokensList.Right + 10, _tokensList.Top + 24),
                Font           = GameService.Content.DefaultFont32,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                StrokeText     = true,
                Parent         = buildPanel
            };

            var bullet = GameService.Content.GetTexture("155038");

            int offset = 18;

            var step1Bullet = new Image(bullet) {
                Size     = new Point(16,                16),
                Location = new Point(instructions.Left, instructions.Bottom + 10),
                Parent   = buildPanel
            };

            var step1 = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.CreateTokenInstructions_Step1,
                Location       = new Point(step1Bullet.Right + 2, step1Bullet.Top - 3),
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            var openAnetApplicationsBttn = new StandardButton() {
                Text     = Strings.GameServices.Gw2ApiService.Link_ManageApplications,
                Icon     = GameService.Content.GetTexture("common/1441452"),
                Size     = new Point(256,        32),
                Location = new Point(step1.Left, step1.Bottom + 5),
                Parent   = buildPanel
            };

            var step2Bullet = new Image(bullet) {
                Size     = new Point(16,                16),
                Location = new Point(step1Bullet.Left, openAnetApplicationsBttn.Bottom + offset),
                Parent   = buildPanel
            };

            var step2 = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.CreateTokenInstructions_Step2,
                Location       = new Point(step2Bullet.Right + 2, step2Bullet.Top - 3),
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            var step3Bullet = new Image(bullet) {
                Size     = new Point(16,               16),
                Location = new Point(step1Bullet.Left, step2.Bottom + offset),
                Parent   = buildPanel
            };

            var step3 = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.CreateTokenInstructions_Step3,
                Location       = new Point(step3Bullet.Right + 2, step3Bullet.Top - 3),
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            var step3Warn = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.CreateTokenInstructions_Warning,
                TextColor      = Control.StandardColors.Yellow,
                Location       = new Point(step3.Left, step3.Bottom + 3),
                Font           = GameService.Content.DefaultFont12,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            var step4Bullet = new Image(bullet) {
                Size     = new Point(16,               16),
                Location = new Point(step1Bullet.Left, step3Warn.Bottom + offset),
                Parent   = buildPanel
            };

            var step4 = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.CreateTokenInstructions_Step4,
                Location       = new Point(step4Bullet.Right + 2, step4Bullet.Top - 3),
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            var step5Bullet = new Image(bullet) {
                Size     = new Point(16,               16),
                Location = new Point(step1Bullet.Left, step4.Bottom + offset),
                Parent   = buildPanel
            };

            var step5 = new Label() {
                Text           = Strings.GameServices.Gw2ApiService.CreateTokenInstructions_Step5,
                Location       = new Point(step5Bullet.Right + 2, step5Bullet.Top - 3),
                Font           = GameService.Content.DefaultFont16,
                AutoSizeWidth  = true,
                AutoSizeHeight = true,
                Parent         = buildPanel
            };

            openAnetApplicationsBttn.Click += delegate { Process.Start("https://account.arena.net/applications"); };

            ReloadApiKeys();

            SetTokenStatus(ApiTokenStatusType.Neutral);
        }

        private void ReloadApiKeys() {
            _tokensList.ClearChildren();

            foreach (var key in GameService.Gw2WebApi.GetKeys()) {
                var nPanel = new ViewContainer() {
                    Size     = new Point(350, 82),
                    ShowTint = true,
                    Parent   = _tokensList
                };

                var apiTokenView      = new ApiTokenView();
                var apiTokenPresenter = new ApiTokenPresenter(apiTokenView, key);

                nPanel.Show(apiTokenView.WithPresenter(apiTokenPresenter));
            }
        }

        private CancellationToken GetToken() {
            _tokenTestCanceller?.Cancel();
            _tokenTestCanceller = new CancellationTokenSource();

            return _tokenTestCanceller.Token;
        }

        private void ApiKeyTextBoxOnTextChanged(object sender, EventArgs e) {
            SetTokenStatus(ApiTokenStatusType.Loading);
            _tokenCheckDebounceWrapper(this.ApiKey);
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
                } catch (BadRequestException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_InvalidToken);
                } catch (InvalidAccessTokenException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_InvalidToken);
                } catch (MissingScopesException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_InvalidToken);
                } catch (AuthorizationRequiredException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_AccountFailed);
                } catch (UnexpectedStatusException) {
                    SetTokenStatus(ApiTokenStatusType.Failed, Strings.Common.Unknown);
                } catch (RequestCanceledException) {
                    // NOOP keep existing status to avoid walking over the call that cancelled us
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

            // Check minimum permissions
            if (_minimumTokenPermissions.Any(minPermission => !tokenTask.Result.Permissions.List.Contains(new ApiEnum<TokenPermission>(minPermission)))) {
                SetTokenStatus(ApiTokenStatusType.Failed, Strings.GameServices.Gw2ApiService.TokenStatus_MissingMinPermission);
                return;
            }

            // Check partial permissions (total minus TokenPermission.Unknown)
            if (tokenTask.Result.Permissions.List.Count < EnumUtil.GetCachedValues<TokenPermission>().Length - 1) {
                SetTokenStatus(ApiTokenStatusType.Partial, Strings.GameServices.Gw2ApiService.TokenStatus_PartialPermission);
                return;
            }

            // Token is valid
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
