using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls;
using Flurl;
using Flurl.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Blish_HUD.Modules.KillProof.Controls;
namespace Blish_HUD.Modules.KillProof
{
    public class KillProof : Module
    {
        private WindowTab KillProofTab;

        private Point LABEL_SMALL = new Point(400, 30);
        private Point LABEL_BIG = new Point(400, 40);
        private Point ICON_SIZE = new Point(64, 64);
        private LabelBase CurrentAccount;
        private LabelBase CurrentAccountLastRefresh;
        private LabelBase CurrentAccountKpId;
        private LabelBase CurrentAccountProofUrl;
        private List<KillProofButton> displayedKillProofs = new List<KillProofButton>();
        private readonly Dictionary<string, string> KillProofRepository = new Dictionary<string, string>{
            {"li","Legendary Insights (LI)"},
            {"ld","Legendary Divinations (LD)"},
            {"ce","Unstable Cosmic Essences"}
        };
        private const string DD_ALL = "KP to Titles";
        private const string DD_RAID = "Raid Titles";
        private const string DD_FRACTAL = "Fractal Titles";
        private const string DD_KILLPROOF = "Kill Proofs";

        private const string KILLPROOF_API_URL = "https://killproof.me/api/";

        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo(
                "Kill Proof",
                GameService.Content.GetTexture("killproof_icon"),
                "bh.general.killproof",
                "Check someone's kills for raiding.",
                "Nekres.1038",
                "1.0"
            );
        }

        #region Settings

        //private SettingEntry<bool> settingExample;

        public override void DefineSettings(Settings settings)
        {
            // Define settings
            //settingExample = settings.DefineSetting<bool>("name", false, false, true, "Description");
        }

        #endregion
        public override void OnEnabled()
        {
            base.OnEnabled();

            KillProofTab = GameService.Director.BlishHudWindow.AddTab("KillProof", GameService.Content.GetTexture("killproof_icon"), BuildHomePanel(GameService.Director.BlishHudWindow), 0);
        }

        private Panel BuildHomePanel(WindowBase wndw)
        {
            var hPanel = new Panel()
            {
                CanScroll = false,
                Size = wndw.ContentRegion.Size
            };

            // HEADER
            var header = new Panel()
            {
                Parent = hPanel,
                Size = new Point(hPanel.Width, 200),
                Location = new Point(0, 0),
                CanScroll = false
            };
            var img_killproof = new Image(GameService.Content.GetTexture("killproof_logo"))
            {
                Parent = header,
                Size = new Point(128, 128),
                Location = new Point(Panel.LEFT_MARGIN, -25)
            };
            var lab_account_name = new LabelBase()
            {
                Parent = header,
                Size = new Point(200, 30),
                Location = new Point(header.Width / 2 - 100, Panel.TOP_MARGIN),
                Text = "Account Name or KillProof.me-ID:"
            };
            var tb_account_name = new TextBox()
            {
                Parent = header,
                Size = new Point(200, 30),
                Location = new Point(header.Width / 2 - 100, lab_account_name.Bottom + Panel.BOTTOM_MARGIN),
                PlaceholderText = "Player.0000",

            };
            CurrentAccount = new LabelBase()
            {
                Parent = header,
                Size = LABEL_BIG,
                Location = new Point(Panel.LEFT_MARGIN, img_killproof.Bottom + Panel.BOTTOM_MARGIN),
                ShowShadow = true,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular),
                Text = ""
            };
            CurrentAccountLastRefresh = new LabelBase()
            {
                Parent = header,
                Size = LABEL_SMALL,
                Location = new Point(Panel.LEFT_MARGIN, CurrentAccount.Bottom + Panel.BOTTOM_MARGIN),
                Text = ""
            };
            var ddSortMethod = new Dropdown()
            {
                Parent = header,
                Visible = header.Visible,
                Width = 150,
                Location = new Point(header.Right - 150 - Panel.RIGHT_MARGIN, CurrentAccountLastRefresh.Location.Y)
            };
            ddSortMethod.Items.Add(DD_ALL);
            ddSortMethod.Items.Add(DD_KILLPROOF);
            ddSortMethod.Items.Add(DD_RAID);
            ddSortMethod.Items.Add(DD_FRACTAL);
            ddSortMethod.ValueChanged += UpdateSort;
            ddSortMethod.SelectedItem = DD_ALL;
            // FOOTER
            var footer = new Panel()
            {
                Parent = hPanel,
                Size = new Point(hPanel.Width, 50),
                Location = new Point(0, hPanel.Height - 50),
                CanScroll = false
            };
            CurrentAccountKpId = new LabelBase()
            {
                Parent = footer,
                Size = LABEL_SMALL,
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Left,
                Location = new Point(Panel.LEFT_MARGIN, (footer.Height / 2) - (LABEL_SMALL.Y / 2)),
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size8, ContentService.FontStyle.Regular),
                Text = ""
            };
            CurrentAccountProofUrl = new LabelBase()
            {
                Parent = footer,
                Size = LABEL_SMALL,
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Left,
                Location = new Point(Panel.LEFT_MARGIN, (footer.Height / 2) - (LABEL_SMALL.Y / 2) + Panel.BOTTOM_MARGIN),
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size8, ContentService.FontStyle.Regular),
                Text = ""
            };
            var creditLabel = new LabelBase()
            {
                Parent = footer,
                Size = LABEL_SMALL,
                HorizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Center,
                Location = new Point((footer.Width / 2) - (LABEL_SMALL.X / 2), (footer.Height / 2) - (LABEL_SMALL.Y / 2)),
                Text = @"Powered by www.killproof.me"
            };

            var contentPanel = new TintedPanel()
            {
                Parent = hPanel,
                Size = new Point(header.Size.X, hPanel.Height - header.Height - footer.Height),
                Location = new Point(0, header.Bottom),
                ShowBorder = true,
                CanScroll = true
            };

            tb_account_name.OnEnterPressed += delegate {
                if (tb_account_name != null && !string.Equals(tb_account_name.Text, "") && !Regex.IsMatch(tb_account_name.Text, @"[^a-zA-Z0-9.\s]|^\.*$"))
                {
                    BuildContentPanelElements(contentPanel, tb_account_name.Text);
                    UpdateSort(ddSortMethod, EventArgs.Empty);
                }
            };
            return hPanel;
        }
        private async Task<string> GetKillProofContent(string _account)
        {
            var rawJson = await (KILLPROOF_API_URL + $"kp/{_account}")
                .AllowAnyHttpStatus()
                .GetStringAsync();
            return rawJson;
        }
        private Panel BuildContentPanelElements(Panel contentPanel, string account)
        {
            var loader = Task.Run(() => GetKillProofContent(account));
            loader.Wait();

            var data = JObject.Parse(loader.Result);

            foreach (KillProofButton e1 in displayedKillProofs) { e1.Dispose(); }
            displayedKillProofs.Clear();
            if ((string)data["error"] != null)
            {
                CurrentAccount.Text = (string)data["error"];
                CurrentAccountLastRefresh.Text = "";
                CurrentAccountKpId.Text = "";
                CurrentAccountProofUrl.Text = "";
            }
            else
            {
                CurrentAccount.Text = (string)data["account_name"] != null ? (string)data["account_name"] : "";
                var refreshDate = (DateTime)data["last_refresh"];
                CurrentAccountLastRefresh.Text = (string)data["last_refresh"] != null ? "Last Refresh: " + String.Format("{0:dddd, d. MMMM yyyy - HH:mm:ss}", refreshDate) : "";
                CurrentAccountKpId.Text = (string)data["kpid"] != null ? "ID: " + (string)data["kpid"] : "";
                CurrentAccountProofUrl.Text = (string)data["proof_url"] != null ? (string)data["proof_url"] : "";

                var killproofs = JsonConvert.DeserializeObject<Dictionary<string, int>>(data["killproofs"].ToString());

                foreach (KeyValuePair<string, int> token in killproofs)
                {
                    if (token.Value > 0)
                    {
                        var killProofButton = new KillProofButton()
                        {
                            Parent = contentPanel,
                            Icon = GameService.Content.GetTexture("icon_" + token.Key),
                            Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular),
                            Text = token.Value.ToString(),
                            BottomText = KillProofRepository[token.Key]
                        };
                        displayedKillProofs.Add(killProofButton);
                    }
                }


                var titles = JsonConvert.DeserializeObject<Dictionary<string, string>>(data["titles"].ToString());

                foreach (KeyValuePair<string, string> token in titles)
                {
                    var titleButton = new KillProofButton()
                    {
                        Parent = contentPanel,
                        Icon = GameService.Content.GetTexture("icon_" + token.Value),
                        Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size16, ContentService.FontStyle.Regular),
                        Text = token.Key,
                        Title = token.Value
                    };
                    System.Console.WriteLine(token.Value);
                    displayedKillProofs.Add(titleButton);
                }


                RepositionKp();
            }
            return contentPanel;
        }
        private void UpdateSort(object sender, EventArgs e)
        {
            switch (((Dropdown)sender).SelectedItem)
            {
                case DD_ALL:
                    displayedKillProofs.Sort((e1, e2) => e1.BottomText.CompareTo(e2.BottomText));
                    foreach (KillProofButton e1 in displayedKillProofs) { e1.Visible = true; }
                    break;
                case DD_KILLPROOF:
                    displayedKillProofs.Sort((e1, e2) => e1.BottomText.CompareTo(e2.BottomText));
                    foreach (KillProofButton e1 in displayedKillProofs) { e1.Visible = e1.Title == ""; }
                    break;
                case DD_FRACTAL:
                    displayedKillProofs.Sort((e1, e2) => e1.Text.CompareTo(e2.Text));
                    foreach (KillProofButton e1 in displayedKillProofs) { e1.Visible = e1.Title.ToLower().Contains("fractal"); }
                    break;
                case DD_RAID:
                    displayedKillProofs.Sort((e1, e2) => e1.Text.CompareTo(e2.Text));
                    foreach (KillProofButton e1 in displayedKillProofs) { e1.Visible = e1.Title.ToLower().Contains("raid"); }
                    break;
                default:
                    throw new NotSupportedException();
            }

            RepositionKp();
        }
        private void RepositionKp()
        {
            int pos = 0;
            foreach (var kp in displayedKillProofs)
            {
                int x = pos % 3;
                int y = pos / 3;
                kp.Location = new Point(x * 335, y * 108);

                ((Panel)kp.Parent).VerticalScrollOffset = 0;
                kp.Parent.Invalidate();
                if (kp.Visible) pos++;
            }
        }
        public override void OnDisabled()
        {
            sampleBuffer.Clear();
            GameService.Director.BlishHudWindow.RemoveTab(KillProofTab);
        }
        private long lastUpdate = 0;
        private Queue<double> sampleBuffer = new Queue<double>();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Unless we're in game running around, don't show.
            if (!GameService.GameIntegration.IsInGame)
            {
                return;
            }
            lastUpdate = GameService.Gw2Mumble.UiTick;
        }
    }
}
