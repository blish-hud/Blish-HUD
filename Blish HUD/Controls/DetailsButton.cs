using System;
using System.ComponentModel;
using System.Linq;
using Blish_HUD.Controls.Effects;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public enum DetailsIconSize {
        /// <summary>
        /// The icon will fill the <see cref="DetailsButton"/> from the top down to the content area at the bottom.
        /// </summary>
        Small,
        /// <summary>
        /// The icon will fill the <see cref="DetailsButton"/> from top to bottom on the left side.
        /// </summary>
        Large
    }

    public enum DetailsHighlightType {
        /// <summary>
        /// Mousing over the <see cref="DetailsButton"/> will not cause any highlight or animation to be shown.
        /// </summary>
        None,
        /// <summary>
        /// Mousing over the <see cref="DetailsButton"/> will cause a scrolling animation to occur in the background of the control.
        /// This is the default when <see cref="DetailsDisplayMode.Standard"/> is the active display mode.
        /// </summary>
        ScrollingHighlight,
        /// <summary>
        /// Mousing over the <see cref="DetailsButton"/> will make the background slightly lighter.
        /// This is the default when <see cref="DetailsDisplayMode.Summary"/> or <see cref="DetailsDisplayMode.Completed"/> is the active display mode.
        /// </summary>
        LightHighlight
    }

    public enum DetailsDisplayMode {
        /// <summary>
        /// Based on the achievement control when the achievement has not been completed yet, this view displays a content region for <see cref="GlowButton"/>s,
        /// can have a fill, and the option of a corner icon.  This is the default display mode.
        /// </summary>
        Standard,
        /// <summary>
        /// [NOT IMPLEMENTED] Based on the achievement controls shown on the Summary tab, this view only displays the <see cref="DetailsButton.Text"/>,
        /// optionally displays a fill, and the fill fraction or <see cref="DetailsButton.IconDetails"/>.
        /// </summary>
        Summary,
        /// <summary>
        /// [NOT IMPLEMENTED] Based on the achievement control when the achievement has been completed.  A burst is shown behind the <see cref="DetailsButton.Icon"/>
        /// and the text and <see cref="DetailsButton.IconDetails"/> are shown to the right of it.
        /// </summary>
        Completed
    }

    /// <summary>
    /// Used to show details and multiple options for a single topic. Designed to look like the Achievements details icon.
    /// </summary>
    public class DetailsButton : FlowPanel {

        private const int DEFAULT_EVENTSUMMARY_WIDTH   = 354;
        private const int DEFAULT_EVENTSUMMARY_HEIGHT  = 100;
        private const int DEFAULT_BOTTOMSECTION_HEIGHT = 35;

        #region Load Static

        private static readonly Texture2D _textureFillCrest;
        private static readonly Texture2D _textureVignette;
        private static readonly Texture2D _textureCornerButton;
        private static readonly Texture2D _textureBottomSectionSeparator;

        static DetailsButton() {
            _textureFillCrest              = Content.GetTexture(@"controls/detailsbutton/605004");
            _textureVignette               = Content.GetTexture(@"controls/detailsbutton/605003");
            _textureCornerButton           = Content.GetTexture(@"controls/detailsbutton/605011");
            _textureBottomSectionSeparator = Content.GetTexture(@"157218");
        }

        #endregion

        private DetailsDisplayMode   _displayMode = DetailsDisplayMode.Standard;
        private DetailsIconSize      _iconSize    = DetailsIconSize.Large;
        private string               _text;
        private string               _iconDetails;
        private AsyncTexture2D       _icon;
        private bool                 _showVignette = true;
        private int                  _maxFill;
        private int                  _currentFill;
        private bool                 _showFillFraction;
        private Color                _fillColor           = Color.LightGray;
        private DetailsHighlightType _highlightType       = DetailsHighlightType.ScrollingHighlight;
        private int                  _bottomSectionHeight = DEFAULT_BOTTOMSECTION_HEIGHT;
        private bool                 _showToggleButton    = false;
        private bool                 _toggleState         = false;

        /// <summary>
        /// Determines the way the <see cref="DetailsButton"/> will render.
        /// </summary>
        public DetailsDisplayMode DisplayMode {
            get => _displayMode;
            set => SetProperty(ref _displayMode, value);
        }

        /// <summary>
        /// Determines how big the icon should be in the <see cref="DetailsButton"/>.
        /// This property changes the layout of the <see cref="DetailsButton"/>, somewhat.
        /// </summary>
        public DetailsIconSize IconSize {
            get => _iconSize;
            set => SetProperty(ref _iconSize, value);
        }

        /// <summary>
        /// The text displayed on the right side of the <see cref="DetailsButton"/>.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        /// <summary>
        /// The icon to display on the left side of the <see cref="DetailsButton"/>.
        /// </summary>
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        /// <summary>
        /// If fill is not enabled or if <see cref="ShowFillFraction"/> is false, then this text will display under the <see cref="Icon"/>.
        /// </summary>
        public string IconDetails {
            get => _iconDetails;
            set => SetProperty(ref _iconDetails, value);
        }

        /// <summary>
        /// Whether to show a vignette around the <see cref="Icon"/> or not. A vignette is required for a fill to show.
        /// </summary>
        public bool ShowVignette {
            get => _showVignette;
            set => SetProperty(ref _showVignette, value);
        }

        /// <summary>
        /// The maximum the <see cref="CurrentFill"/> can be set to.  If <see cref="ShowVignette"/>
        /// is true,then setting this value to a value greater than 0 will enable the fill.
        /// </summary>
        public int MaxFill {
            get => _maxFill;
            set => SetProperty(ref _maxFill, value);
        }

        /// <summary>
        /// The current fill progress.  The maximum value is clamped to <see cref="MaxFill"/>.
        /// </summary>
        public int CurrentFill {
            get => _currentFill;
            set {
                if (SetProperty(ref _currentFill, Math.Min(value, _maxFill))) {
                    _animFill?.Cancel();
                    _animFill = null;

                    _animFill = Animation.Tweener.Tween(this, new { DisplayedFill = _currentFill }, 0.65f, 0, true).Ease(Glide.Ease.QuintIn);
                }
            }
        }

        /// <summary>
        /// If enabled, the last child control will be aligned to the right side.
        /// </summary>
        public bool ShowToggleButton {
            get => _showToggleButton;
            set => SetProperty(ref _showToggleButton, value, true);
        }

        /// <summary>
        /// Normally paired with <see cref="ShowToggleButton"/>.  If enabled, a glow will be shown in the bottom right.
        /// </summary>
        public bool ToggleState {
            get => _toggleState;
            set => SetProperty(ref _toggleState, value);
        }

        /// <summary>
        /// Do not directly manipulate this property.  It is only public because the animation library requires it to be public.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float DisplayedFill { get; set; } = 0;

        /// <summary>
        /// If true, <see cref="MaxFill"/> > 0, and <see cref="ShowFillFraction"/> is true, a fraction will be shown under the <see cref="Icon"/>
        /// instead of <see cref="IconDetails"/> of the format "<see cref="CurrentFill"/> / <see cref="MaxFill"/>."
        /// </summary>
        public bool ShowFillFraction {
            get => _showFillFraction;
            set => SetProperty(ref _showFillFraction, value);
        }

        /// <summary>
        /// The <see cref="Color"/> of the fill.
        /// </summary>
        public Color FillColor {
            get => _fillColor;
            set => SetProperty(ref _fillColor, value);
        }

        /// <summary>
        /// The way the <see cref="DetailsButton"/> will animate or highlight when moused over.
        /// </summary>
        public DetailsHighlightType HighlightType {
            get => _highlightType;
            set {
                if (SetProperty(ref _highlightType, value) && this.EffectBehind != null) {
                    this.EffectBehind.Enabled = _highlightType == DetailsHighlightType.ScrollingHighlight;
                }
            }
        }

        /// <summary>
        /// The height of the bottom content region.  The default is 35.
        /// </summary>
        public int BottomSectionHeight {
            get => _bottomSectionHeight;
            set => SetProperty(ref _bottomSectionHeight, value, true);
        }

        private          Glide.Tween              _animFill;
        private readonly ScrollingHighlightEffect _scrollEffect;

        public DetailsButton() {
            this.Size = new Point(DEFAULT_EVENTSUMMARY_WIDTH, DEFAULT_EVENTSUMMARY_HEIGHT);
            
            this.ControlPadding = new Vector2(6, 1);
            this.PadLeftBeforeControl = true;
            this.PadTopBeforeControl = true;

            _scrollEffect = new ScrollingHighlightEffect(this) {
                Enabled = (_highlightType == DetailsHighlightType.ScrollingHighlight)
            };

            this.EffectBehind = _scrollEffect;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.Filter;
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            _scrollEffect.SetEnableState(
                                         _highlightType == DetailsHighlightType.ScrollingHighlight
                                            && (this.RelativeMousePosition.Y < this.ContentRegion.Top
                                            ||  this.RelativeMousePosition.X < this.ContentRegion.Left)
                                        );

            base.OnMouseMoved(e);
        }

        public override void RecalculateLayout() {
            int bottomRegionLeft = _size.Y;

            if (this.IconSize == DetailsIconSize.Small) {
                bottomRegionLeft = 0;

                if (!this.ShowVignette) {
                    bottomRegionLeft += 10;
                }
            }

            this.ContentRegion = new Rectangle(bottomRegionLeft,
                                               this.Height - _bottomSectionHeight,
                                               this.Width - bottomRegionLeft,
                                               _bottomSectionHeight);

            if (_showToggleButton && _children.Any(c => c.Visible)) {
                var lastControl = _children.FindLast(c => c.Visible);

                lastControl.Right = this.ContentRegion.Width - 4;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // TODO: Move all calculations into RecalculateLayout()

            float backgroundOpacity = (this.MouseOver && _highlightType == DetailsHighlightType.LightHighlight) ? 0.1f : 0.25f;

            // Draw background
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   bounds,
                                   Color.Black * backgroundOpacity);

            int iconSize = this.IconSize == DetailsIconSize.Large
                               ? _size.Y
                               : _size.Y - _bottomSectionHeight;

            int iconOffset = this.IconSize == DetailsIconSize.Large
                                 ? 0
                                 : !_showVignette ? 10 : 0;

            float fillPercent = _maxFill > 0
                                    ? this.DisplayedFill / _maxFill
                                    : 1f;

            float fillSpace = iconSize * fillPercent;

            // Draw bottom section
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(this.ContentRegion.X - iconOffset, this.ContentRegion.Y, this.ContentRegion.Width + iconOffset, this.ContentRegion.Height),
                                   Color.Black * 0.1f);

            /*** Handle fill ***/
            if (_maxFill > 0 && _showVignette) {
                // Draw icon twice
                if (_icon != null) {
                    float localIconFill = (fillSpace - iconSize / 2f + 32) / 64;

                    // Icon above the fill
                    if (localIconFill < 1)
                        spriteBatch.DrawOnCtrl(this,
                                               _icon,
                                               new Rectangle(
                                                             iconSize / 2 - 64 / 2 + iconOffset,
                                                             iconSize / 2          - 64 / 2,
                                                             64,
                                                             64 - (int)(64 * localIconFill)
                                                            ),
                                               new Rectangle(0, 0, 64, 64 - (int)(64 * localIconFill)),
                                               Color.DarkGray * 0.4f);

                    // Icon below the fill
                    if (localIconFill > 0)
                        spriteBatch.DrawOnCtrl(
                                               this,
                                               _icon,
                                               new Rectangle(
                                                             iconSize / 2 - 64 / 2 + iconOffset,
                                                             iconSize / 2 - 64 / 2  + (64 - (int)(localIconFill * 64)),
                                                             64,
                                                             (int)(localIconFill * 64)
                                                            ),
                                               new Rectangle(0, 64 - (int)(localIconFill * 64), 64, (int)(localIconFill * 64))
                                              );
                }

                if (_currentFill > 0) {
                    // Draw the fill
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, (int)(iconSize - fillSpace), iconSize, (int)(fillSpace)), _fillColor * 0.3f);

                    // Only show the fill crest if we aren't full
                    if (fillPercent < 0.99)
                        spriteBatch.DrawOnCtrl(this, _textureFillCrest,  new Rectangle(0, iconSize - (int) (fillSpace), iconSize, iconSize));
                }

                if (_showFillFraction)
                    spriteBatch.DrawStringOnCtrl(this, $"{_currentFill}/{_maxFill}", Content.DefaultFont14, new Rectangle(0, 0, iconSize, (int)(iconSize * 0.99f)), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Bottom);
            } else if (_icon != null) {
                // Draw icon without any fill effects
                spriteBatch.DrawOnCtrl(
                                       this,
                                       _icon,
                                       new Rectangle(iconSize / 2 - 64 / 2 + iconOffset,
                                                     iconSize / 2          - 64 / 2,
                                                     64,
                                                     64)
                                      );
            }

            if (!_showFillFraction || !(_maxFill > 0 && _showVignette)) {
                spriteBatch.DrawStringOnCtrl(this, _iconDetails, Content.DefaultFont14, new Rectangle(0, 0, iconSize, (int)(iconSize * 0.99f)), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Bottom);
            }

            // Draw icon vignette (draw with or without the icon to keep a consistent look)
            if (_showVignette)
                spriteBatch.DrawOnCtrl(this,
                                       _textureVignette,
                                       new Rectangle(0, 0, iconSize, iconSize));

            // Draw toggle icon background
            if (_showToggleButton && _children.Any(c => c.Visible)) {
                var lastControl = _children.FindLast(c => c.Visible);

                var cornerStart = new Point(lastControl.Left - 4 + this.ContentRegion.X,
                                            _size.Y - _bottomSectionHeight);

                spriteBatch.DrawOnCtrl(this,
                                       _textureCornerButton,
                                       new Rectangle(cornerStart.X,
                                                     cornerStart.Y,
                                                     _size.X - cornerStart.X,
                                                     _size.Y - cornerStart.Y),
                                       Color.Black);
            }

            // Draw bottom section seperator
            spriteBatch.DrawOnCtrl(this,
                                   _textureBottomSectionSeparator,
                                   this.IconSize == DetailsIconSize.Large
                                       ? new Rectangle(this.ContentRegion.Left, _size.Y - _bottomSectionHeight - _textureBottomSectionSeparator.Height / 2, this.ContentRegion.Width, _textureBottomSectionSeparator.Height)
                                       : new Rectangle(0, _size.Y - _bottomSectionHeight - _textureBottomSectionSeparator.Height / 2, _size.X, _textureBottomSectionSeparator.Height));

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, _text, Content.DefaultFont14, new Rectangle(iconSize + 20, 0, _size.X - iconSize - 35, this.Height - _bottomSectionHeight), Color.White, true, true);
        }

    }
}
