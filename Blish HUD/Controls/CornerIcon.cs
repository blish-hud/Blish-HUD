using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {
    public class CornerIcon : Control {

        public enum CornerIconAlignment {
            Left,
            Center,
        }

        private const int ICON_POSITION = 10;
        private const int ICON_SIZE = 32;
        private const float ICON_TRANS = 0.4f;

        private Texture2D _icon;
        public Texture2D Icon {
            get => _icon;
            set {
                if (_icon == value) return;

                _icon = value;
                OnPropertyChanged();
            }
        }

        private Texture2D _hoverIcon;
        public Texture2D HoverIcon {
            get => _hoverIcon;
            set {
                if (_hoverIcon == value) return;

                _hoverIcon = value;
                OnPropertyChanged();
            }
        }
        
        private float _hoverTrans = ICON_TRANS;
        public float HoverTrans {
            get => _hoverTrans;
            set {
                if (_hoverTrans == value) return;

                _hoverTrans = value;
                OnPropertyChanged();
            }
        }

        private bool _mouseInHouse = false;
        public bool MouseInHouse {
            get => _mouseInHouse;
            set {
                if (_mouseInHouse == value) return;

                _mouseInHouse = value;
                
                Animation.Tweener.Tween(this, new { HoverTrans = (this.MouseInHouse ? 1f : ICON_TRANS) }, 0.45f);

                OnPropertyChanged();
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

        private int _priority;
        public int Priority {
            get => _priority;
            set {
                if (_priority == value) return;

                _priority = value;
                UpdateCornerIconPositions();
            }
        }

        static CornerIcon() {
            CornerIcons = new ObservableCollection<CornerIcon>();

            CornerIcons.CollectionChanged += delegate { UpdateCornerIconPositions(); };

            GameService.Input.MouseMoved += (sender, e) => {
                if (Alignment == CornerIconAlignment.Left) {
                    var scaledMousePos = e.MouseState.Position.ScaleToUi();
                    if (scaledMousePos.Y < Overlay.Form.Top + ICON_SIZE && scaledMousePos.X < ICON_SIZE * (ICON_POSITION + CornerIcons.Count) + LeftOffset) {
                        foreach (var cornerIcon in CornerIcons) {
                            cornerIcon.MouseInHouse = true;
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
            List<CornerIcon> sortedIcons = CornerIcons.OrderBy((cornerIcon) => cornerIcon.Priority).ToList();

            int horizontalOffset = Alignment == CornerIconAlignment.Left ? ICON_SIZE * ICON_POSITION + LeftOffset : Graphics.SpriteScreen.Width / 2 - (CornerIcons.Count * ICON_SIZE / 2);

            for (int i = 0; i < CornerIcons.Count; i++) {
                sortedIcons[i].Location = new Point(ICON_SIZE * i + horizontalOffset, 0);
            }
        }

        public CornerIcon() {
            this.Parent = Graphics.SpriteScreen;
            this.Size = new Point(ICON_SIZE, ICON_SIZE);

            CornerIcons.Add(this);
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            Content.PlaySoundEffectByName(@"audio\button-click");
        }

        // TODO: Use a shader to replace "HoverIcon"
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.HoverIcon == null) return;

            if (this.MouseOver) {
                if (this.HoverIcon == null) {
                    var srcBounds = bounds;
                    srcBounds.Inflate(1.5f, 1.5f);

                    spriteBatch.Draw(this.Icon, bounds, Color.White);
                } else {
                    spriteBatch.Draw(this.HoverIcon, bounds, Color.White);
                }
            } else {
                spriteBatch.Draw(this.Icon, bounds, Color.White * this.HoverTrans);
            }
        }
    }
}
