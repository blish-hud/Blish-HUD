using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;

namespace Blish_HUD.Controls {
    public class CornerIcon : Container {

        public enum CornerIconAlignment {
            Left,
            Center,
        }

        private const int   ICON_POSITION = 10;
        private const int   ICON_SIZE     = 32;
        private const float ICON_TRANS    = 0.4f;

        private AsyncTexture2D _icon;
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private AsyncTexture2D _hoverIcon;
        public AsyncTexture2D HoverIcon {
            get => _hoverIcon;
            set => SetProperty(ref _hoverIcon, value);
        }
        
        private float _hoverTrans = ICON_TRANS;
        public float HoverTrans {
            get => _hoverTrans;
            set => SetProperty(ref _hoverTrans, value);
        }

        private bool _mouseInHouse = false;
        public bool MouseInHouse {
            get => _mouseInHouse;
            set {
                if (SetProperty(ref _mouseInHouse, value)) {
                    Animation.Tweener.Tween(this, new {HoverTrans = (this.MouseInHouse ? 1f : ICON_TRANS)}, 0.45f);
                }
            }
        }

        private static int _leftOffset = 0;
        public static int LeftOffset {
            get => _leftOffset;
            set {
                if (_leftOffset == value) return;

                _leftOffset = value;
                UpdateCornerIconPositions();
            }
        }

        private static CornerIconAlignment _alignment = CornerIconAlignment.Left;
        public static CornerIconAlignment Alignment {
            get => _alignment;
            set {
                if (_alignment == value) return;

                _alignment = value;
                UpdateCornerIconPositions();
            }
        }

        private static ObservableCollection<CornerIcon> CornerIcons { get; }

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
        public string LoadingMessage {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value, true);
        }

        static CornerIcon() {
            CornerIcons = new ObservableCollection<CornerIcon>();

            CornerIcons.CollectionChanged += delegate { UpdateCornerIconPositions(); };

            GameService.Input.MouseMoved += (sender, e) => {
                if (Alignment == CornerIconAlignment.Left) {
                    var scaledMousePos = e.MouseState.Position.ScaleToUi();
                    if (scaledMousePos.Y < BlishHud.Form.Top + ICON_SIZE && scaledMousePos.X < ICON_SIZE * (ICON_POSITION + CornerIcons.Count) + LeftOffset) {
                        foreach (var cornerIcon in CornerIcons) {
                            cornerIcon.MouseInHouse = scaledMousePos.X < cornerIcon.Left || cornerIcon.MouseOver;
                        }

                        return;
                    }
                }

                foreach (var cornerIcon in CornerIcons) {
                    cornerIcon.MouseInHouse = false;
                }
            };
        }

        private static void UpdateCornerIconPositions() {
            List<CornerIcon> sortedIcons = CornerIcons.OrderByDescending((cornerIcon) => cornerIcon.Priority).ToList();

            int horizontalOffset = Alignment == CornerIconAlignment.Left ? ICON_SIZE * ICON_POSITION + LeftOffset : Graphics.SpriteScreen.Width / 2 - (CornerIcons.Count * ICON_SIZE / 2);

            for (int i = 0; i < CornerIcons.Count; i++) {
                sortedIcons[i].Location = new Point(ICON_SIZE * i + horizontalOffset, 0);
            }
        }

        private readonly LoadingSpinner _iconLoader;
        public CornerIcon() {
            this.Parent        = Graphics.SpriteScreen;
            this.Size          = new Point(ICON_SIZE, ICON_SIZE);
            this.ContentRegion = new Rectangle(0, ICON_SIZE, ICON_SIZE, ICON_SIZE);

            _iconLoader = new LoadingSpinner() {
                Parent = this,
                Size   = this.ContentRegion.Size,
            };

            CornerIcons.Add(this);
        }

        /// <inheritdoc />
        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        /// <inheritdoc />
        protected override void OnClick(MouseEventArgs e) {
            Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnClick(e);
        }

        private Rectangle _layoutIconBounds;
        /// <inheritdoc />

        public override void RecalculateLayout() {
            _layoutIconBounds = new Rectangle(0, 0, ICON_SIZE, ICON_SIZE);

            bool isLoading = !string.IsNullOrEmpty(_loadingMessage);
            this.Size = new Point(ICON_SIZE, isLoading ? ICON_SIZE * 2 : ICON_SIZE);
            _iconLoader.Visible = isLoading;
            _iconLoader.BasicTooltipText = _loadingMessage;
        }

        // TODO: Use a shader to replace "HoverIcon"
        /// <inheritdoc />
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_icon == null) return;

            if (this.MouseOver && this.RelativeMousePosition.Y <= _layoutIconBounds.Bottom) {
                spriteBatch.DrawOnCtrl(this, _hoverIcon ?? _icon, _layoutIconBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, _icon, _layoutIconBounds, Color.White * _hoverTrans);
            }
        }

        /// <inheritdoc />
        protected override void DisposeControl() {
            CornerIcons.Remove(this);
        }

    }
}
