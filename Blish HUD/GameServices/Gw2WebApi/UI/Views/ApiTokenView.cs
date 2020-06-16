using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Gw2WebApi.UI.Views {
    public class ApiTokenView : View {

        private readonly Dictionary<string, (string Region, Texture2D Flag)> _worldRegionFlags = new Dictionary<string, (string Region, Texture2D Flag)>() {
            {"10", ("US / North America", GameService.Content.GetTexture(@"common/784343"))},
            {"20", ("Europe", GameService.Content.GetTexture(@"common/784346"))},
            {"21", ("France", GameService.Content.GetTexture(@"common/784345"))},
            {"22", ("Germany", GameService.Content.GetTexture(@"common/784342"))},
            {"23", ("Spain", GameService.Content.GetTexture(@"common/784344"))}
        };

        private Label _accountNameLbl;
        private Label _tokenKeyLbl;
        private Label _tokenNameLbl;
        private Label _failedTokenLbl;

        private Image _regionFlagImg;
        private Image _accountCommanderImg;

        private GlowButton _deleteBttn;

        private bool _errored;
        private bool _active;

        private TokenInfo                _tokenInfo;
        private Account                  _accountInfo;
        private IApiV2ObjectList<string> _characterList;

        public TokenInfo TokenInfo {
            get => _tokenInfo;
            set => SetTokenInfo(value);
        }

        public Account AccountInfo {
            get => _accountInfo;
            set => SetAccountInfo(value);
        }

        public IApiV2ObjectList<string> CharacterList {
            get => _characterList;
            set => SetCharacterList(value);
        }

        public bool CanDelete {
            get => _deleteBttn.Visible;
            set => _deleteBttn.Visible = value;
        }

        public bool Errored {
            get => _errored;
            set {
                _errored = value;

                if (_accountNameLbl != null) {
                    _accountNameLbl.Visible = !_errored;
                    _tokenKeyLbl.Visible    = !_errored;
                    _tokenNameLbl.Visible   = !_errored;

                    _failedTokenLbl.Visible = _errored;
                }
            }
        }

        public bool Active {
            get => _active;
            set => SetActive(value);
        }

        public void SetTokenInfo(TokenInfo tokenInfo) {
            _tokenInfo = tokenInfo;

            _tokenNameLbl.Text             = _tokenInfo.Name;
            _tokenNameLbl.BasicTooltipText = _tokenInfo.Name;
        }

        public void SetAccountInfo(Account accountInfo) {
            _accountInfo = accountInfo;

            _accountNameLbl.Text = _accountInfo.Name;

            // Set flag for region
            if (_worldRegionFlags.TryGetValue(_accountInfo.World.ToString().Substring(0, 2), out var regionInfo)) {
                _regionFlagImg.Texture          = regionInfo.Flag;
                _regionFlagImg.BasicTooltipText = regionInfo.Region;
            }

            // Set commander status
            _accountCommanderImg.Visible = _accountInfo.Commander;
        }

        public void SetCharacterList(IApiV2ObjectList<string> characterList) {
            _characterList = characterList;

            _tokenKeyLbl.Text             = Strings.GameServices.Gw2ApiService.AccountInfo_Character.ToQuantity(_characterList.Count);
            _tokenKeyLbl.BasicTooltipText = string.Join("\n", _characterList);
        }

        private void SetActive(bool active) {
            _active = active;

            this.ViewTarget.BackgroundTexture = _active
                                                    ? GameService.Content.GetTexture("1060353-crop")
                                                    : GameService.Content.GetTexture("1863945-crop");
        }

        protected override void Build(Panel buildPanel) {
            _failedTokenLbl = new Label() {
                Size                = buildPanel.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Middle,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia,
                                                   ContentService.FontSize.Size14,
                                                   ContentService.FontStyle.Italic),
                Text    = Strings.GameServices.Gw2ApiService.TokenStatus_FailedToLoad,
                Visible = _errored,
                Parent  = buildPanel
            };

            _accountNameLbl = new Label() {
                Text           = "[Account Name]",
                Font           = GameService.Content.DefaultFont16,
                ShowShadow     = true,
                Location       = new Point(10, 10),
                AutoSizeHeight = true,
                Width          = buildPanel.Width - 20,
                Visible        = !_errored,
                Parent         = buildPanel
            };

            _regionFlagImg = new Image() {
                Size     = new Point(16,                   16),
                Location = new Point(_accountNameLbl.Left, _accountNameLbl.Bottom + 2),
                Visible  = false,
                Parent   = buildPanel
            };

            _accountCommanderImg = new Image(GameService.Content.GetTexture("common/1234943")) {
                Size             = new Point(16,                       16),
                Location         = new Point(_regionFlagImg.Right + 4, _regionFlagImg.Top),
                BasicTooltipText = Strings.GameServices.Gw2ApiService.AccountInfo_Commander,
                Visible          = false,
                Parent           = buildPanel
            };

            _tokenKeyLbl = new Label() {
                Text           = "[Token Key]",
                AutoSizeHeight = true,
                Width          = buildPanel.Width / 4,
                Left           = _accountNameLbl.Left,
                Bottom         = buildPanel.Height - 10,
                Visible        = !_errored,
                Parent         = buildPanel
            };

            _tokenNameLbl = new Label() {
                Text                = "[Token Name]",
                HorizontalAlignment = HorizontalAlignment.Right,
                AutoSizeHeight      = true,
                Width               = (buildPanel.Width / 4) * 3 - 30,
                Left                = _tokenKeyLbl.Right         + 10,
                Bottom              = _tokenKeyLbl.Bottom,
                Visible             = !_errored,
                Parent              = buildPanel
            };

            _deleteBttn = new GlowButton() {
                Icon             = GameService.Content.GetTexture("common/733269"),
                ActiveIcon       = GameService.Content.GetTexture("common/733270"),
                Location         = new Point(buildPanel.Width - 26, 10),
                Size             = new Point(16,                    16),
                BasicTooltipText = Strings.GameServices.Gw2ApiService.ManageApiKeys_DeleteToken,
                Parent           = buildPanel
            };

            _deleteBttn.Click += DeleteRegisteredToken;
        }

        private void DeleteRegisteredToken(object sender, MouseEventArgs e) {
            GameService.Gw2WebApi.UnregisterKey(_tokenInfo.Id);

            this.ViewTarget.Dispose();
        }

    }
}
