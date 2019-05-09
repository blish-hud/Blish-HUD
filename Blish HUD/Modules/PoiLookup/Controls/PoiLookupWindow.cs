using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Blish_HUD.Modules.PoiLookup {

    // TODO: This should be updated to allow any number of possible results be displayed
    public class PoiLookupWindow : Controls.WindowBase {

        private const int WINDOW_WIDTH = 256; //196;
        private const int WINDOW_HEIGHT = 178;

        private const ContentService.FontFace TOOLTIP_FONT_FAMILY = ContentService.FontFace.Menomonia;
        private const ContentService.FontSize TOOLTIP_FONT_SIZE = ContentService.FontSize.Size14;
        private const ContentService.FontSize TOOLTIP_SMALLFONT_SIZE = ContentService.FontSize.Size12;

        public struct WordScoreResult {
            public BHGw2Api.Landmark Landmark { get; set; }
            public int DiffScore { get; set; }

            public WordScoreResult(BHGw2Api.Landmark landmark, int diffScore) {
                this.Landmark = landmark;
                this.DiffScore = diffScore;
            }
        }

        private PoiItem _currentPoiItem;
        private PoiItem CurrentPoiItem {
            get => _currentPoiItem;
            set {
                if (_currentPoiItem == value) return;

                _currentPoiItem = value;

                _result1.Active = _result1 == _currentPoiItem;
                _result2.Active = _result2 == _currentPoiItem;
                _result3.Active = _result3 == _currentPoiItem;
            }
        }

        Controls.TextBox Searchbox;

        private PoiItem _result1;
        private PoiItem _result2;
        private PoiItem _result3;

        private ToolTip _resultDetails;

        private readonly PoiLookup Module;

        public PoiLookupWindow(PoiLookup module) : base() {
            Module = module;

            TitleBarHeight = 32;
            this.Size = new Point(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.Title = "";
            this.ZIndex = Controls.Screen.TOOLWINDOW_BASEZINDEX;
            ExitBounds = new Rectangle(this.Width - 32, 0, 32, 32);

            Searchbox = new Controls.TextBox();
            Searchbox.PlaceholderText = "Search";
            Searchbox.Location = new Point(0, TitleBarHeight);
            Searchbox.Size = new Point(this.Width, Searchbox.Height);
            Searchbox.Parent = this;

            // Tooltip used by all three result items
            var ttDetails1 = new Controls.Tooltip();
            var ttDetailsLmName = new Controls.LabelBase() {
                Text              = "Name Loading...",
                Font              = Content.DefaultFont16,
                Location          = new Point(10, 10),
                Height            = 11,
                TextColor         = ContentService.Colors.Chardonnay,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1
            };

            var ttDetailsInfHint1 = new Controls.LabelBase() {
                Text              = "Enter: Copy landmark to clipboard.",
                Font              = Content.DefaultFont16,
                Location          = new Point(10, ttDetailsLmName.Bottom + 5),
                TextColor         = Color.White,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1
            };

            var ttDetailsInf1 = new Controls.LabelBase() {
                Text              = "Closest Waypoint",
                Font              = Content.DefaultFont16,
                Location          = new Point(10, ttDetailsInfHint1.Bottom + 12),
                Height            = 11,
                TextColor         = ContentService.Colors.Chardonnay,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1
            };

            var ttDetailsInfRes1 = new Controls.LabelBase() {
                Text              = " ",
                Font              = Content.DefaultFont14,
                Location          = new Point(10, ttDetailsInf1.Bottom + 5),
                TextColor         = Color.White,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1
            };

            var ttDetailsInfHint2 = new Controls.LabelBase() {
                Text              = "Shift + Enter: Copy closest waypoint to clipboard.",
                Font              = Content.DefaultFont14,
                Location          = new Point(10, ttDetailsInfRes1.Bottom + 5),
                TextColor         = Color.White,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1,
                Visible           = false
            };

            // Result items

            _result1 = new PoiItem {
                Icon     = Content.GetTexture("60976"),
                Visible  = false,
                Location = new Point(2, TitleBarHeight + Searchbox.Height),
                Size     = new Point(this.Width        - 4, 37),
                Tooltip  = ttDetails1,
                Parent   = this
            };

            _result2 = new PoiItem {
                Icon     = Content.GetTexture("60976"),
                Visible  = false,
                Location = new Point(2, TitleBarHeight + Searchbox.Height + 39),
                Size     = new Point(this.Width                           - 4, 37),
                Tooltip  = ttDetails1,
                Parent   = this
            };

            _result3 = new PoiItem {
                Icon     = Content.GetTexture("60976"),
                Visible  = false,
                Location = new Point(2, TitleBarHeight + Searchbox.Height + 78),
                Size     = new Point(this.Width                           - 4, 37),
                Tooltip  = ttDetails1,
                Parent   = this
            };

            void ResultCtrl_Activated(object sender, PropertyChangedEventArgs args) {
                if (args.PropertyName == "Active") {
                    var currItem = (PoiItem) sender;

                    if (currItem.Active) {
                        this.CurrentPoiItem = currItem;

                        ttDetails1.CurrentControl = currItem;

                        ttDetailsLmName.Text = this.CurrentPoiItem.Name;

                        var closestLandmark = Module.GetClosestWaypoint(currItem.Landmark);

                        if (closestLandmark != null) {
                            ttDetailsInfRes1.Font = Content.GetFont(TOOLTIP_FONT_FAMILY, TOOLTIP_FONT_SIZE, ContentService.FontStyle.Regular);
                            ttDetailsInfRes1.Text = closestLandmark.Name;
                        } else {
                            ttDetailsInfRes1.Font = Content.GetFont(TOOLTIP_FONT_FAMILY, TOOLTIP_FONT_SIZE, ContentService.FontStyle.Italic);
                            ttDetailsInfRes1.Text = "none found";
                        }

                        if (!currItem.MouseOver) {
                            ttDetails1.Location = new Point(currItem.AbsoluteBounds.Right + 5, currItem.AbsoluteBounds.Top);
                        }

                        ttDetails1.Visible = true;
                    }
                }
            }

            bool ctrlDown = false;
            Searchbox.OnKeyDown += delegate (object sender, Keys keys) {
                if (keys == Keys.LeftControl || keys == Keys.RightControl) ctrlDown = true;
            };
            Searchbox.OnKeyUp += delegate (object sender, Keys keys) {
                if (keys == Keys.LeftControl || keys == Keys.RightControl) ctrlDown = false;
            };

            _result1.PropertyChanged += ResultCtrl_Activated;
            _result2.PropertyChanged += ResultCtrl_Activated;
            _result3.PropertyChanged += ResultCtrl_Activated;

            _result1.LeftMouseButtonReleased += delegate { ResultCtrl_Submitted(_result1, ctrlDown); };
            _result2.LeftMouseButtonReleased += delegate { ResultCtrl_Submitted(_result2, ctrlDown); };
            _result3.LeftMouseButtonReleased += delegate { ResultCtrl_Submitted(_result3, ctrlDown); };

            Searchbox.OnEnterPressed += delegate {
                if (_result1.Visible)
                    ResultCtrl_Submitted(this.CurrentPoiItem, ctrlDown);
            };

            Searchbox.OnKeyPressed += delegate(object sender, Keys keys) {
                if (keys == Keys.Down) {
                    if (this.CurrentPoiItem == null && _result1.Visible) {
                        this.CurrentPoiItem = _result1;
                    } else if(this.CurrentPoiItem == _result1 && _result2.Visible) {
                        this.CurrentPoiItem = _result2;
                    } else if (this.CurrentPoiItem == _result2 && _result3.Visible) {
                        this.CurrentPoiItem = _result3;
                    }
                } else if (keys == Keys.Up) {
                    // We don't need to check if these ones are visible since if the one below is visible
                    // the one above it must also be visible, anyways
                    if (this.CurrentPoiItem == _result3) {
                        this.CurrentPoiItem = _result2;
                    } else if (this.CurrentPoiItem == _result2) {
                        this.CurrentPoiItem = _result1;
                    }
                } else {
                    // They've continued to type something - bring it back to the first result
                    this.CurrentPoiItem = _result1.Visible ? _result1 : null;
                }
            };

            Searchbox.OnTextChanged += SearchBox_TextChanged;
        }

        // TODO: Split out as async and show spinner
        private void SearchBox_TextChanged(object sender, EventArgs e) {
            var poiCtrls = new List<PoiItem> { _result1, _result2, _result3 };
            poiCtrls.ForEach(ctrl => ctrl.Visible = false);

            if (Searchbox.Text.Length > 0) {
                var landmarkDiffs = new List<WordScoreResult>();

                foreach (var landmark in Module.PointsOfInterest.Values) {
                    string lmName = landmark.Name.ToLower();
                    string sValue = Searchbox.Text.ToLower();
                    
                    int score;

                    if (lmName.StartsWith(sValue))
                        score = 0;
                    else if (lmName.EndsWith(sValue))
                        score = 3;
                    else
                        score = Utils.String.ComputeLevenshteinDistance(sValue, lmName.Substring(0, Math.Min(Searchbox.Text.Length, landmark.Name.Length)));

                    landmarkDiffs.Add(new WordScoreResult(landmark, score));
                }

                var possibleLocations = landmarkDiffs.OrderBy(x => x.DiffScore).ToList().Take(3).ToList();

                int i = 0;
                foreach (var validLoc in possibleLocations) {
                    var curCtrl = poiCtrls[i];
                    curCtrl.Visible = true;
                    curCtrl.Landmark = validLoc.Landmark;

                    i++;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, Content.GetTexture("156390"), new Rectangle(Point.Zero, _size));
            spriteBatch.DrawStringOnCtrl(this, 
                                           "Landmark Search",
                                           Content.DefaultFont14,
                                           new Rectangle(8, 0, ExitBounds.Left - 16, TitleBarHeight),
                                           Color.White);

            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        private void ResultCtrl_Submitted(PoiItem item, bool copyAlt) {
            // TODO: Lots of this code can be reduced / cleaned up
            if (copyAlt) {
                try {
                    var closestLandmark = Module.GetClosestWaypoint(item.Landmark);
                    Clipboard.SetText(closestLandmark.ChatLink);

                    if (Module.settingShowNotificationWhenLandmarkIsCopied.Value)
                        Controls.Notification.ShowNotification(item.Icon, $"{closestLandmark.Type} copied to clipboard.", 2);
                } catch (Exception ex) {
                    // TODO: Notify properly here. This rarely happens, but we shouldn't let it crash the app.
                    Controls.Notification.ShowNotification(item.Icon, "Failed to copy to clipboard. Try again?", 2);
                }
            } else {
                try {
                    Clipboard.SetText(item.Landmark.ChatLink);
                    
                    if (Module.settingShowNotificationWhenLandmarkIsCopied.Value)
                        Controls.Notification.ShowNotification(item.Icon, $"{item.Landmark.Type} copied to clipboard.", 2);
                } catch (Exception ex) {
                    // TODO: Notify properly here. This rarely happens, but we shouldn't let it crash the app.
                    Controls.Notification.ShowNotification(item.Icon, "Failed to copy to clipboard. Try again?", 2);
                }
            }

            Searchbox.Text = "";

            if (Module.settingHideWindowAfterSelection.Value)
                Hide();

            GameService.GameIntegration.FocusGw2();
        }

    }
}
