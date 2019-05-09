using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blish_HUD.Utils;
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
        /// Based on the achievement controls shown on the Summary tab, this view only displays the <see cref="DetailsButton.Text"/>,
        /// optionally displays a fill, and the fill fraction or <see cref="DetailsButton.IconDetails"/>.
        /// </summary>
        Summary,
        /// <summary>
        /// Based on the achievement control when the achievement has been completed.  A burst is shown behind the <see cref="DetailsButton.Icon"/>
        /// and the text and <see cref="DetailsButton.IconDetails"/> are shown to the right of it.
        /// </summary>
        Completed
    }

    /// <summary>
    /// Used to show details and multiple options for a single topic. Designed to look like the Achievements details icon.
    /// </summary>
    public class DetailsButton : FlowPanel {

        private const int EVENTSUMMARY_WIDTH  = 354;
        private const int EVENTSUMMARY_HEIGHT = 100;
        private const int BOTTOMSECTION_HEIGHT = 35;

        private DetailsDisplayMode _displayMode = DetailsDisplayMode.Standard;
        private DetailsIconSize _iconSize = DetailsIconSize.Large;
        private string _text;
        private string _iconDetails;
        private Texture2D _icon;
        private bool _showVignette = true;
        private int _maxFill;
        private int _currentFill;
        private bool _showFillFraction;
        private Color _fillColor = Color.LightGray;
        private DetailsHighlightType _highlightType = DetailsHighlightType.ScrollingHighlight;

        /// <summary>
        /// Determines the way the <see cref="DetailsButton"/> will render.
        /// </summary>
        public DetailsDisplayMode Displaymode {
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
        public Texture2D Icon {
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
                    _fillAnim?.Cancel();
                    _fillAnim = null;

                    float diffLength = Math.Abs(this.DisplayedFill - _currentFill) / (float)this.MaxFill;

                    _fillAnim = Animation.Tweener.Tween(this, new { DisplayedFill = _currentFill }, 0.65f, 0, true).Ease(Glide.Ease.QuintIn);
                }
            }
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
                if (SetProperty(ref _highlightType, value)) {
                    if (this.EffectBehind != null) 
                        this.EffectBehind.Enabled = _highlightType == DetailsHighlightType.ScrollingHighlight;
                }
            }
        }

        private Glide.Tween _fillAnim;

        public DetailsButton() {
            this.Size = new Point(EVENTSUMMARY_WIDTH, EVENTSUMMARY_HEIGHT);
            
            this.ControlPadding = new Vector2(6, 1);
            this.PadLeftBeforeControl = true;
            this.PadTopBeforeControl = true;

            this.EffectBehind = new Effects.ScrollingHighlightEffect(this) {
                Size    = _size.ToVector2(),
                Enabled = (_highlightType == DetailsHighlightType.ScrollingHighlight)
            };
        }

        public override void RecalculateLayout() {
            int bottomRegionLeft = EVENTSUMMARY_HEIGHT;

            if (this.IconSize == DetailsIconSize.Small) {
                bottomRegionLeft = 0;

                if (!this.ShowVignette) {
                    bottomRegionLeft += 10;
                }
            }

            this.ContentRegion = new Rectangle(bottomRegionLeft,
                                               this.Height - BOTTOMSECTION_HEIGHT,
                                               this.Width - bottomRegionLeft,
                                               BOTTOMSECTION_HEIGHT);

            if (this.EffectBehind != null) {
                ((Effects.ScrollingHighlightEffect)this.EffectBehind).ActiveZones.Clear();
                ((Effects.ScrollingHighlightEffect)this.EffectBehind).ActiveZones.Add(new Rectangle(0, 0, _size.X, this.ContentRegion.Top));
                ((Effects.ScrollingHighlightEffect)this.EffectBehind).ActiveZones.Add(new Rectangle(0, 0, this.ContentRegion.Left, _size.Y));
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            float backgroundOpacity = (this.MouseOver && _highlightType == DetailsHighlightType.LightHighlight) ? 0.1f : 0.25f;

            // Draw background
            spriteBatch.DrawOnCtrl(
                                   this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(Point.Zero, _size),
                                   Color.Black * backgroundOpacity
                                  );

            int iconSize = this.IconSize == DetailsIconSize.Large
                               ? EVENTSUMMARY_HEIGHT
                               : EVENTSUMMARY_HEIGHT - BOTTOMSECTION_HEIGHT;

            int iconOffset = this.IconSize == DetailsIconSize.Large
                                 ? 0
                                 : !_showVignette ? 10 : 0;

            float fillPercent = _maxFill > 0
                                    ? (float)this.DisplayedFill / _maxFill
                                    : 1f;

            float fillSpace = iconSize * fillPercent;

            // Draw bottom section
            spriteBatch.DrawOnCtrl(
                                   this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(this.ContentRegion.X - iconOffset, this.ContentRegion.Y, this.ContentRegion.Width + iconOffset, this.ContentRegion.Height),
                                   Color.Black * 0.1f
                                  );

            /*** Handle fill ***/
            if (_maxFill > 0 && _showVignette) {
                // Draw icon twice
                if (this.Icon != null) {

                    float localIconFill = (fillSpace - iconSize / 2f + 32) / 64;

                    // Icon above the fill
                    if (localIconFill < 1)
                        spriteBatch.DrawOnCtrl(
                                               this,
                                               _icon,
                                               new Rectangle(
                                                             iconSize / 2 - 64 / 2 + iconOffset,
                                                             iconSize / 2          - 64 / 2,
                                                             64,
                                                             64 - (int)(64 * localIconFill)
                                                            ),
                                               new Rectangle(0, 0, 64, 64 - (int)(64 * localIconFill)),
                                               Color.DarkGray * 0.4f
                                              );

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
                        spriteBatch.DrawOnCtrl(this, Content.GetTexture("605004"),  new Rectangle(0, iconSize - (int) (fillSpace), iconSize, iconSize));
                }

                if (_showFillFraction)
                    spriteBatch.DrawStringOnCtrl(this, $"{_currentFill}/{_maxFill}", Content.DefaultFont14, new Rectangle(0, 0, iconSize, (int)(iconSize * 0.99f)), Color.White, true, true, 1, DrawUtil.HorizontalAlignment.Center, DrawUtil.VerticalAlignment.Bottom);
            } else if (_icon != null) {
                // Draw icon without any fill effects
                spriteBatch.DrawOnCtrl(
                                       this,
                                       _icon,
                                       new Rectangle(
                                                     iconSize / 2 - 64 / 2 + iconOffset,
                                                     iconSize / 2          - 64 / 2,
                                                     64,
                                                     64
                                                    )
                                      );
            }

            if (!_showFillFraction || !(_maxFill > 0 && _showVignette)) {
                spriteBatch.DrawStringOnCtrl(this, _iconDetails, Content.DefaultFont14, new Rectangle(0, 0, iconSize, (int)(iconSize * 0.99f)), Color.White, true, true, 1, DrawUtil.HorizontalAlignment.Center, DrawUtil.VerticalAlignment.Bottom);
            }

            // Draw icon vignette (draw with or without the icon to keep a consistent look)
            //if (this.IconSize == DetailsIconSize.Large)
            if (_showVignette)
                spriteBatch.DrawOnCtrl(
                                       this,
                                       Content.GetTexture("605003"),
                                       new Rectangle(0, 0, iconSize, iconSize)
                                      );

            // Draw bottom section seperator
            spriteBatch.DrawOnCtrl(
                                   this,
                                   Content.GetTexture("157218"),
                                   this.IconSize == DetailsIconSize.Large
                                   ? new Rectangle(this.ContentRegion.Left, _size.Y - 39, this.ContentRegion.Width, 8)
                                   : new Rectangle(0, _size.Y - 39, _size.X, 8)
                                  );

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, _text, Content.DefaultFont14, new Rectangle(iconSize + 20, 0, EVENTSUMMARY_WIDTH - iconSize - 35, this.Height - BOTTOMSECTION_HEIGHT), Color.White, true, true);
        }

    }
}
