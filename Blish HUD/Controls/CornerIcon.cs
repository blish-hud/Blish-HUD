using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Blish_HUD.Graphics;

namespace Blish_HUD.Controls {
    public class CornerIcon : Control {

        private static int _leftOffset = 0;
        public static int LeftOffset {
            get => _leftOffset;
            set {
                if (_leftOffset == value) return;

                _leftOffset = value;
                UpdateCornerIconPositions();
            }
        }

        private static ObservableCollection<CornerIcon> CornerIcons { get; }

        private static readonly Rectangle _standardIconBounds;

        private const int   ICON_POSITION = 10;
        private const int   ICON_SIZE     = 32;
        private const float ICON_TRANS    = 0.6f;
        
        private float _hoverTrans = ICON_TRANS;
        public float HoverTrans {
            get => this.Enabled ? _hoverTrans : ICON_TRANS;
            set => SetProperty(ref _hoverTrans, value);
        }

        private bool _mouseInHouse = false;
        public bool MouseInHouse {
            get => _mouseInHouse;
            set {
                if (SetProperty(ref _mouseInHouse, value)) {
                    Animation.Tweener.Tween(this, new { HoverTrans = (this.MouseInHouse ? 1f : ICON_TRANS) }, 0.45f);
                }
            }
        }

        private bool _dynamicHide = false;
        public bool DynamicHide {
            get => _dynamicHide;
            set {
                if (SetProperty(ref _dynamicHide, value)) {
                    Animation.Tweener.Tween(this, new { HoverTrans = (this.DynamicHide ? ICON_TRANS : 0.0f) }, (this.DynamicHide ? 0.55f : 0.65f)).OnBegin(() => {
                        if (this.DynamicHide) this.Visible = true;
                    }).OnComplete(() => {
                        if (!this.DynamicHide) this.Visible = false;
                    });
                }
            }
        }

        private AsyncTexture2D _icon;
        /// <summary>
        /// The icon shown when the <see cref="CornerIcon"/> is not currently being hovered over.
        /// </summary>
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private AsyncTexture2D _hoverIcon;
        /// <summary>
        /// The icon shown when the <see cref="CornerIcon"/> is hovered over.
        /// </summary>
        public AsyncTexture2D HoverIcon {
            get => _hoverIcon;
            set => SetProperty(ref _hoverIcon, value);
        }

        private string _iconName;
        /// <summary>
        /// The name of the <see cref="CornerIcon"/> that is shown when moused over.
        /// </summary>
        public string IconName {
            get => _iconName;
            set => SetProperty(ref _iconName, value);
        }

        private int? _priority;
        /// <summary>
        /// <see cref="CornerIcon"/>s are sorted by priority so that, from left to right, priority goes from the highest to lowest.
        /// </summary>
        public int Priority {
            get => _priority ?? (_icon?.GetHashCode() ?? 0);
            set {
                if (SetProperty(ref _priority, value)) {
                    UpdateCornerIconPositions();
                }
            }
        }

        private string _loadingMessage;
        /// <summary>
        /// If defined, a loading spinner is shown below the <see cref="CornerIcon"/> and this text will be
        /// shown in a tooltip when the loading spinner is moused over.
        /// </summary>
        public string LoadingMessage {
            get => _loadingMessage;
            set {
                if (SetProperty(ref _loadingMessage, value, true) && _mouseOver) {
                    this.BasicTooltipText = _loadingMessage;
                }
            }
        }

        static CornerIcon() {
            CornerIcons = new ObservableCollection<CornerIcon>();

            _standardIconBounds = new Rectangle(0, 0, ICON_SIZE, ICON_SIZE);

            CornerIcons.CollectionChanged += delegate { UpdateCornerIconPositions(); };
            
            GameService.Input.Mouse.MouseMoved += (sender, e) => {
                var scaledMousePos = Input.Mouse.State.Position.ScaleToUi();
                if (scaledMousePos.Y < ICON_SIZE && scaledMousePos.X < ICON_SIZE * (ICON_POSITION + CornerIcons.Count - 1) + LeftOffset) {
                    foreach (var cornerIcon in CornerIcons) {
                        cornerIcon.MouseInHouse = true;
                    }

                    return;
                }

                foreach (var cornerIcon in CornerIcons) {
                    cornerIcon.MouseInHouse = false;
                }
            };

            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += delegate { UpdateCornerIconDynamicHUDState(); };
        }

        private static void UpdateCornerIconPositions() {
            List<CornerIcon> sortedIcons = CornerIcons.OrderByDescending((cornerIcon) => cornerIcon.Priority).ToList();

            int horizontalOffset = ICON_SIZE * ICON_POSITION + LeftOffset;

            for (int i = 0; i < CornerIcons.Count; i++) {
                sortedIcons[i].Location = new Point(ICON_SIZE * i + horizontalOffset, 0);
            }
        }

        public static void UpdateCornerIconDynamicHUDState() {
            if (GameService.Overlay.DynamicHUDMenuBar == DynamicHUDMethod.ShowPeaceful && GameService.Gw2Mumble.PlayerCharacter.IsInCombat) {
                foreach (var cornerIcon in CornerIcons) {
                    cornerIcon.DynamicHide = false;
                }
            } else {
                foreach (var cornerIcon in CornerIcons) {
                    cornerIcon.DynamicHide = true;
                }
            }
        }

        public CornerIcon() {
            this.Parent = Graphics.SpriteScreen;
            this.Size   = new Point(ICON_SIZE, ICON_SIZE);
            this.DynamicHide = true;

            CornerIcons.Add(this);
        }

        public CornerIcon(AsyncTexture2D icon, string iconName) : this() {
            _icon     = icon;
            _iconName = iconName;
        }

        public CornerIcon(AsyncTexture2D icon, AsyncTexture2D hoverIcon, string iconName) : this(icon, iconName) {
            _hoverIcon = hoverIcon;
        }

        /// <inheritdoc />
        protected override void OnClick(MouseEventArgs e) {
            Content.PlaySoundEffectByName(@"button-click");

            base.OnClick(e);
        }

        private bool _isLoading = false;

        /// <inheritdoc />
        public override void RecalculateLayout() {
            _isLoading = !string.IsNullOrEmpty(_loadingMessage);
            _size = new Point(ICON_SIZE, _isLoading ? ICON_SIZE * 2 : ICON_SIZE);
        }

        /// <inheritdoc />
        protected override void OnMouseMoved(MouseEventArgs e) {
            if (_isLoading && _mouseOver && this.RelativeMousePosition.Y >= _standardIconBounds.Bottom) {
                this.BasicTooltipText = _loadingMessage;
            } else if (this.Tooltip == null) {
                this.BasicTooltipText = _iconName;
            }

            base.OnMouseMoved(e);
        }

        // TODO: Use a shader to replace "HoverIcon"
        /// <inheritdoc />
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_icon == null) return;

            if (this.MouseOver && this.RelativeMousePosition.Y <= _standardIconBounds.Bottom && this.Enabled) {
                spriteBatch.DrawOnCtrl(this, _hoverIcon ?? _icon, _standardIconBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, _icon, _standardIconBounds, Color.White * this.HoverTrans);
            }

            if (_isLoading) {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(0, ICON_SIZE, ICON_SIZE, ICON_SIZE));
            }
        }

        /// <inheritdoc />
        protected override void DisposeControl() {
            CornerIcons.Remove(this);
        }

    }
}
