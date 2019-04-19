using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Screen = Blish_HUD.Controls.Screen;

namespace Blish_HUD.Modules.PoiLookup {

    // TODO: This should be updated to allow any number of possible results be displayed
    public class PoiLookupWindow : Controls.Window {

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

                Result1.Active = Result1 == _currentPoiItem;
                Result2.Active = Result2 == _currentPoiItem;
                Result3.Active = Result3 == _currentPoiItem;
                
                //_currentPoiItem?.TriggerMouseInput(MouseEventType.MouseMoved,
                //                                   new MouseState(
                //                                                  _currentPoiItem.AbsoluteBounds.Location.X + 5,
                //                                                  _currentPoiItem.AbsoluteBounds.Location.Y + 5,
                //                                                  0,
                //                                                  ButtonState.Released,
                //                                                  ButtonState.Released,
                //                                                  ButtonState.Released,
                //                                                  ButtonState.Released,
                //                                                  ButtonState.Released
                //                                                 ));
            }
        }

        Controls.Textbox Searchbox;

        private PoiItem Result1;
        private PoiItem Result2;
        private PoiItem Result3;

        private ToolTip ResultDetails;

        private readonly PoiLookup Module;

        public PoiLookupWindow(PoiLookup module) : base() {
            Module = module;

            TitleBarHeight = 32;
            this.Size = new Point(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.Title = "";
            this.ZIndex = Controls.Screen.TOOLWINDOW_BASEZINDEX;
            ExitBounds = new Rectangle(this.Width - 32, 0, 32, 32);

            Searchbox = new Controls.Textbox();
            Searchbox.PlaceholderText = "Search";
            Searchbox.Location = new Point(0, TitleBarHeight);
            Searchbox.Size = new Point(this.Width, Searchbox.Height);
            Searchbox.Parent = this;

            // Tooltip used by all three result items

            var ttDetails1 = new Controls.Tooltip();
            var ttDetailsLmName = new Controls.Label() {
                Text              = "Name Loading...",
                Font              = Content.GetFont(TOOLTIP_FONT_FAMILY, ContentService.FontSize.Size16, ContentService.FontStyle.Regular),
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

            var ttDetailsInfHint1 = new Controls.Label() {
                Text              = "Enter: Copy landmark to clipboard.",
                Font              = Content.GetFont(TOOLTIP_FONT_FAMILY, TOOLTIP_FONT_SIZE, ContentService.FontStyle.Regular),
                Location          = new Point(10, ttDetailsLmName.Bottom + 5),
                TextColor         = Color.White,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1
            };


            var ttDetailsInf1 = new Controls.Label() {
                Text              = "Closest Waypoint",
                Font              = Content.GetFont(TOOLTIP_FONT_FAMILY, ContentService.FontSize.Size16, ContentService.FontStyle.Regular),
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

            var ttDetailsInfRes1 = new Controls.Label() {
                Text              = " ",
                Font              = Content.GetFont(TOOLTIP_FONT_FAMILY, TOOLTIP_FONT_SIZE, ContentService.FontStyle.Regular),
                Location          = new Point(10, ttDetailsInf1.Bottom + 5),
                TextColor         = Color.White,
                ShadowColor       = Color.Black,
                ShowShadow        = true,
                AutoSizeWidth     = true,
                AutoSizeHeight    = true,
                VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle,
                Parent            = ttDetails1
            };

            var ttDetailsInfHint2 = new Controls.Label() {
                Text              = "Shift + Enter: Copy closest waypoint to clipboard.",
                Font              = Content.GetFont(TOOLTIP_FONT_FAMILY, TOOLTIP_FONT_SIZE, ContentService.FontStyle.Regular),
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

            Result1 = new PoiItem {
                Icon     = Content.GetTexture("60976"),
                Visible  = false,
                Location = new Point(2, TitleBarHeight + Searchbox.Height),
                Size     = new Point(this.Width        - 4, 37),
                Tooltip  = ttDetails1,
                Parent   = this
            };

            Result2 = new PoiItem {
                Icon     = Content.GetTexture("60976"),
                Visible  = false,
                Location = new Point(2, TitleBarHeight + Searchbox.Height + 39),
                Size     = new Point(this.Width                           - 4, 37),
                Tooltip  = ttDetails1,
                Parent   = this
            };

            Result3 = new PoiItem {
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

            Result1.PropertyChanged += ResultCtrl_Activated;
            Result2.PropertyChanged += ResultCtrl_Activated;
            Result3.PropertyChanged += ResultCtrl_Activated;

            Result1.LeftMouseButtonReleased += delegate { ResultCtrl_Submitted(Result1, ctrlDown); };
            Result2.LeftMouseButtonReleased += delegate { ResultCtrl_Submitted(Result2, ctrlDown); };
            Result3.LeftMouseButtonReleased += delegate { ResultCtrl_Submitted(Result3, ctrlDown); };

            Searchbox.OnEnterPressed += delegate {
                if (Result1.Visible)
                    ResultCtrl_Submitted(this.CurrentPoiItem, ctrlDown);
            };

            Searchbox.OnKeyPressed += delegate(object sender, Keys keys) {
                if (keys == Keys.Down) {
                    if (this.CurrentPoiItem == null && Result1.Visible) {
                        this.CurrentPoiItem = Result1;
                    } else if(this.CurrentPoiItem == Result1 && Result2.Visible) {
                        this.CurrentPoiItem = Result2;
                    } else if (this.CurrentPoiItem == Result2 && Result3.Visible) {
                        this.CurrentPoiItem = Result3;
                    }
                } else if (keys == Keys.Up) {
                    // We don't need to check if these ones are visible since if the one below is visible
                    // the one above it must also be visible, anyways
                    if (this.CurrentPoiItem == Result3) {
                        this.CurrentPoiItem = Result2;
                    } else if (this.CurrentPoiItem == Result2) {
                        this.CurrentPoiItem = Result1;
                    }
                } else {
                    // They've continued to type something - bring it back to the first result
                    this.CurrentPoiItem = Result1.Visible ? Result1 : null;
                }
            };

            Searchbox.OnTextChanged += SearchBox_TextChanged;
        }

        // TODO: Split out as async and show spinner
        private void SearchBox_TextChanged(object sender, EventArgs e) {
            var poiCtrls = new List<PoiItem> { Result1, Result2, Result3 };
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

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(GameServices.GetService<ContentService>().GetTexture("156390"), bounds, Color.White);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, GameServices.GetService<ContentService>().GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular), "Landmark Search", new Rectangle(8, 0, ExitBounds.Left - 16, TitleBarHeight), Color.White, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Middle);

            base.PaintContainer(spriteBatch, bounds);
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
                HideWindow();

            GameService.GameIntegration.FocusGw2();
        }

    }
}
