using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Musician.Controls;
using Blish_HUD.Modules.Musician.Notation.Persistance;
using Blish_HUD.Modules.Musician.Player;
using Blish_HUD.Modules.Musician.Player.Algorithms;
using Blish_HUD.Modules.Musician.Notation.Parsers;
namespace Blish_HUD.Modules.Musician
{
    public class Musician : Module
    {
        private Texture2D ICON = GameService.Content.GetTexture("musician_icon");
        private const string DD_TITLE = "Title";
        private const string DD_ARTIST = "Artist";
        private const string DD_USER = "User";
        private const string DD_HARP = "Harp";
        private const string DD_FLUTE = "Flute";
        private const string DD_LUTE = "Lute";
        private const string DD_HORN = "Horn";
        private const string DD_BASS = "Bass";
        private const string DD_BELL = "Bell";
        private const string DD_BELL2 = "Bell2";
        private List<string> Instruments = new List<string>{
           "Harp", "Flute", "Lute", "Horn", "Bell", "Bell2", "Bass"
        };
        private WindowTab MusicianTab;
        private MusicPlayer MusicPlayer;
        private HealthPoolButton StopButton;
        private XmlMusicSheetReader xmlParser;
        private List<SheetButton> displayedSheets;
        private List<RawMusicSheet> Sheets;
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo(
                "Musician",
                ICON,
                "bh.general.musician",
                "Create, share and play sheet music.",
                "Nekres.1038",
                "0.1"
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

            xmlParser = new XmlMusicSheetReader();
            displayedSheets = new List<SheetButton>();
            MusicianTab = GameService.Director.BlishHudWindow.AddTab("Musician", ICON, BuildHomePanel(GameService.Director.BlishHudWindow), 0);
        }

        private Panel BuildHomePanel(WindowBase wndw)
        {
            var hPanel = new Panel()
            {
                CanScroll = false,
                Size = wndw.ContentRegion.Size
            };

            var contentPanel = new Panel()
            {
                Location = new Point(hPanel.Width - 630, 50),
                Size = new Point(630, hPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN),
                Parent = hPanel,
                CanScroll = true
            };
            var menuSection = new Panel
            {
                ShowBorder = true,
                //Title = "Musician Panel",
                Size = new Point(hPanel.Width - contentPanel.Width - 10, contentPanel.Height + Panel.BOTTOM_MARGIN),
                Location = new Point(Panel.LEFT_MARGIN, 20),
                Parent = hPanel,
            };
            var musicianCategories = new Menu
            {
                Size = menuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent = menuSection
            };
            var lPanel = BuildLibraryPanel(wndw);
            var library = musicianCategories.AddMenuItem("Library");
            library.LeftMouseButtonReleased += delegate { wndw.Navigate(lPanel); };
            var cPanel = BuildComposerPanel(wndw);
            var composer = musicianCategories.AddMenuItem("Composer");
            composer.LeftMouseButtonReleased += delegate { wndw.Navigate(cPanel);};

            return hPanel;
        }
        private Panel BuildLibraryPanel(WindowBase wndw)
        {
            var lPanel = new Panel()
            {
                CanScroll = false,
                Size = wndw.ContentRegion.Size
            };
            var backButton = new BackButton(wndw)
            {
                Text = "Musician",
                NavTitle = "Library",
                Parent = lPanel,
                Location = new Point(20, 20),
            };
            var melodyPanel = new Panel()
            {
                Location = new Point(Panel.LEFT_MARGIN + 20, Panel.BOTTOM_MARGIN + backButton.Bottom),
                Size = new Point(lPanel.Size.X - 50 - Panel.LEFT_MARGIN, lPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN),
                Parent = lPanel,
                CanScroll = true
            };

            // Load local sheet music (*.xml) files.
            Task loader = Task.Run(() => Sheets = xmlParser.LoadDirectory(GameService.FileSrv.BasePath + @"\musician"));
            loader.Wait();

            // TODO: Load a list from online database.
            foreach (RawMusicSheet sheet in Sheets)
            {
                var melody = new SheetButton
                {
                    Parent = melodyPanel,
                    Icon = GameService.Content.GetTexture(@"instruments\" + sheet.Instrument.ToLower()),
                    Artist = sheet.Artist,
                    Title = sheet.Title,
                    // TODO: Use ApiService to get fixed, non-editable account name for User.
                    User = sheet.User,
                    MusicSheet = sheet
                };
                displayedSheets.Add(melody);
                melody.LeftMouseButtonPressed += delegate
                {
                    if (melody.MouseOverPlay)
                    {
                        GameService.Director.BlishHudWindow.Hide();
                        MusicPlayer = MusicPlayerFactory.Create(
                            melody.MusicSheet,
                            KeyboardType.Practice
                        );
                        MusicPlayer.Worker.Start();
                    }
                    if (melody.MouseOverEmulate)
                    {
                        GameService.Director.BlishHudWindow.Hide();
                        MusicPlayer = MusicPlayerFactory.Create(
                            melody.MusicSheet,
                            KeyboardType.Emulated
                        );
                        MusicPlayer.Worker.Start();
                        StopButton = new HealthPoolButton()
                        {
                            Parent = GameService.Graphics.SpriteScreen,
                            Text = "Stop Playback"
                        };
                        StopButton.LeftMouseButtonReleased += delegate
                        {
                            this.StopPlayback();
                        };
                    }
                    if (melody.MouseOverPreview)
                    {
                        if (melody.IsPreviewing)
                        {
                            this.StopPlayback();
                            melody.IsPreviewing = false;
                        }
                        else
                        {
                            foreach (SheetButton other in displayedSheets)
                            {
                                other.IsPreviewing = other == melody ? true : false;
                            }
                            if (MusicPlayer != null) { this.StopPlayback(); }
                            MusicPlayer = MusicPlayerFactory.Create(
                                melody.MusicSheet,
                                KeyboardType.Preview
                            );
                            MusicPlayer.Worker.Start();
                        }
                    }
                };
            }
            var ddSortMethod = new Dropdown()
            {
                Parent = lPanel,
                Visible = melodyPanel.Visible,
                Location = new Point(lPanel.Right - 150 - 10, 5),
                Width = 150
            };
            ddSortMethod.Items.Add(DD_TITLE);
            ddSortMethod.Items.Add(DD_ARTIST);
            ddSortMethod.Items.Add(DD_USER);
            ddSortMethod.Items.Add("------------------");
            ddSortMethod.Items.Add(DD_HARP);
            ddSortMethod.Items.Add(DD_FLUTE);
            ddSortMethod.Items.Add(DD_LUTE);
            ddSortMethod.Items.Add(DD_HORN);
            ddSortMethod.Items.Add(DD_BASS);
            ddSortMethod.Items.Add(DD_BELL);
            ddSortMethod.Items.Add(DD_BELL2);
            ddSortMethod.ValueChanged += UpdateSort;
            ddSortMethod.SelectedItem = DD_TITLE;

            UpdateSort(ddSortMethod, EventArgs.Empty);
            backButton.LeftMouseButtonReleased += (object sender, MouseEventArgs e) => { wndw.NavigateHome(); };

            return lPanel;
        }
        private Panel BuildComposerPanel(WindowBase wndw)
        {
            var cPanel = new Panel()
            {
                CanScroll = false,
                Size = wndw.ContentRegion.Size
            };
            var backButton = new BackButton(wndw)
            {
                Text = "Musician",
                NavTitle = "Composer",
                Parent = cPanel,
                Location = new Point(20, 20),
            };
            var composerPanel = new Panel()
            {
                Location = new Point(Panel.LEFT_MARGIN + 20, Panel.BOTTOM_MARGIN + backButton.Bottom),
                Size = new Point(cPanel.Size.X - 50 - Panel.LEFT_MARGIN, cPanel.Size.Y - 50 - Panel.BOTTOM_MARGIN),
                Parent = cPanel,
                CanScroll = false
            };
            var titleTextBox = new TextBox
            {
                Size = new Point(150, 20),
                Location = new Point(0, 20),
                PlaceholderText = "Title",
                Parent = composerPanel
            };
            var titleArtistLabel = new Label
            {
                Size = new Point(20, 20),
                Location = new Point(titleTextBox.Left + titleTextBox.Width + 20, titleTextBox.Top),
                Text = " - ",
                Parent = composerPanel
            };
            var artistTextBox = new TextBox
            {
                Size = new Point(150, 20),
                Location = new Point(titleArtistLabel.Left + titleArtistLabel.Width + 20, titleArtistLabel.Top),
                PlaceholderText = "Artist",
                Parent = composerPanel
            };
            var userLabel = new Label
            {
                Size = new Point(150, 20),
                Location = new Point(0, titleTextBox.Top + 20 + Panel.BOTTOM_MARGIN),
                Text = "Created by",
                Parent = composerPanel
            };

            // TODO: Make this a non-editable Label and get account name from ApiService.
            var userTextBox = new TextBox
            {
                Size = new Point(150, 20),
                Location = new Point(titleArtistLabel.Left + titleArtistLabel.Width + 20, userLabel.Top),
                PlaceholderText = "User (Nekres.1038)",
                Parent = composerPanel
            };
            var ddInstrumentSelection = new Dropdown()
            {
                Parent = composerPanel,
                Location = new Point(0, userTextBox.Top + 20 + Panel.BOTTOM_MARGIN),
                Width = 150,
            };
            foreach (string item in Instruments)
            {
                ddInstrumentSelection.Items.Add(item);
            }
            var ddAlgorithmSelection = new Dropdown()
            {
                Parent = composerPanel,
                Location = new Point(titleArtistLabel.Left + titleArtistLabel.Width + 20, ddInstrumentSelection.Top),
                Width = 150,
            };
            ddAlgorithmSelection.Items.Add("Favor Notes");
            ddAlgorithmSelection.Items.Add("Favor Chords");
            var tempoLabel = new Label()
            {
                Parent = composerPanel,
                Location = new Point(0, ddInstrumentSelection.Top + 22 + Panel.BOTTOM_MARGIN),
                Size = new Point(150, 20),
                Text = "Beats per minute:"
            };
            var tempoCounterBox = new CounterBox()
            {
                Parent = composerPanel,
                Location = new Point(titleArtistLabel.Left + titleArtistLabel.Width + 20, tempoLabel.Top),
                ValueWidth = 50,
                MaxValue = 200,
                MinValue = 40,
                Numerator = 5,
                Value = 90
            };
            var meterLabel = new Label()
            {
                Parent = composerPanel,
                Location = new Point(0, tempoLabel.Top + 22 + Panel.BOTTOM_MARGIN),
                Size = new Point(150, 20),
                Text = "Notes per beat:"
            };
            var meterCounterBox = new CounterBox()
            {
                Parent = composerPanel,
                Location = new Point(titleArtistLabel.Left + titleArtistLabel.Width + 20, meterLabel.Top),
                ValueWidth = 50,
                MaxValue = 16,
                MinValue = 1,
                Prefix = @"1\",
                Exponential = true,
                Value = 1
            };
            var notationTextBox = new TextBox
            {
                Size = new Point(composerPanel.Width, composerPanel.Height - 300),
                Location = new Point(0, meterCounterBox.Top + 22 + Panel.BOTTOM_MARGIN),
                PlaceholderText = "",
                Parent = composerPanel
            };

            // TODO: Upload button. Upload method that validates music sheet (=> valid account name etc.)
            var saveBttn = new StandardButton()
            {
                Text = "Save",
                Location = new Point(composerPanel.Width - 128 - Panel.RIGHT_MARGIN, notationTextBox.Bottom + 5),
                Width = 128,
                Height = 26,
                Parent = composerPanel
            };

            saveBttn.LeftMouseButtonReleased += (sender, args) => {
                // TODO: Save the notation as XML locally.
            };
            backButton.LeftMouseButtonReleased += (object sender, MouseEventArgs e) => { wndw.NavigateHome(); };
            return cPanel;
        }
        private void UpdateSort(object sender, EventArgs e)
        {
            switch (((Dropdown)sender).SelectedItem)
            {
                case DD_TITLE:
                    displayedSheets.Sort((e1, e2) => e1.Title.CompareTo(e2.Title));
                    foreach (SheetButton e1 in displayedSheets){ e1.Visible = true; }
                    break;
                case DD_ARTIST:
                    displayedSheets.Sort((e1, e2) => e1.Artist.CompareTo(e2.Artist));
                    foreach (SheetButton e1 in displayedSheets) { e1.Visible = true; }
                    break;
                case DD_USER:
                    displayedSheets.Sort((e1, e2) => e1.User.CompareTo(e2.User));
                    foreach (SheetButton e1 in displayedSheets) { e1.Visible = true; }
                    break;
                case DD_HARP:
                    displayedSheets.Sort((e1,e2) => e1.MusicSheet.Instrument.CompareTo(e2.MusicSheet.Instrument));
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_HARP, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                case DD_FLUTE:
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_FLUTE, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                case DD_LUTE:
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_LUTE, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                case DD_HORN:
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_HORN, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                case DD_BASS:
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_BASS, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                case DD_BELL:
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_BELL, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
                case DD_BELL2:
                    foreach (SheetButton e1 in displayedSheets)
                    {
                        e1.Visible = string.Equals(e1.MusicSheet.Instrument, DD_BELL2, StringComparison.InvariantCultureIgnoreCase);
                    }
                    break;
            }

            RepositionMel();
        }
        private void RepositionMel()
        {
            int pos = 0;
            foreach (var mel in displayedSheets)
            {
                int x = pos % 3;
                int y = pos / 3;
                mel.Location = new Point(x * 288, y * 108);

                ((Panel)mel.Parent).VerticalScrollOffset = 0;
                mel.Parent.Invalidate();
                if (mel.Visible) pos++;
            }
        }
        public override void OnDisabled()
        {
            sampleBuffer.Clear();
            this.StopPlayback();
            GameService.Director.BlishHudWindow.RemoveTab(MusicianTab);
        }
        private long lastUpdate = 0;
        private Queue<double> sampleBuffer = new Queue<double>();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Unless we're in game running around, don't show.
            if (GameService.GameIntegration.Gw2IsRunning)
            {
                if (Utils.Window.GetForegroundWindow() != GameService.GameIntegration.Gw2Process.MainWindowHandle)
                {
                    this.StopPlayback();
                    return;
                }
            }
            lastUpdate = GameService.Gw2Mumble.UiTick;
        }
        private void StopPlayback()
        {
            if (StopButton != null)
            {
                StopButton.Dispose();
                StopButton = null;
            }
            if (MusicPlayer != null)
            {
                MusicPlayer.Worker.Abort();
                MusicPlayer = null;
            }
        }
    }
}
