using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Blish_HUD.BHGw2Api;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {

    // TODO: Need to have events updated in ColorBox to match the standard applied in Control class
    // TODO: Need to revisit the implementation of ColorBox
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorBox:Control {

        public event EventHandler<EventArgs> OnColorChanged;
        public event EventHandler<EventArgs> OnSelected;

        private const int COLOR_SIZE = 32;

        private bool _selected = false;
        public bool Selected {
            get => _selected;
            set {
                if (SetProperty(ref _selected, value)) {
                    this.OnSelected?.Invoke(this, EventArgs.Empty);
                    if (this.Visible)
                        Content.PlaySoundEffectByName(@"audio\color-change");
                }
            }
        }

        private DyeColor _color;

        public DyeColor Color {
            get => _color;
            set {
                if (_color == value) return;

                _color = value;
                _colorId = value?.Id ?? -1;

                OnPropertyChanged("ColorId");
                OnPropertyChanged();
                this.OnColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _colorId;
        public int ColorId {
            get { return _colorId; }
            set {
                if (_colorId == value) return;
                
                Task<DyeColor> aDye = BHGw2Api.DyeColor.GetById(this.ColorId);
                aDye.ContinueWith(clr => {
                    if (!clr.IsFaulted) {
                        _colorId = value;
                        _color = aDye.Result;

                        OnPropertyChanged();
                        OnPropertyChanged("Color");
                        this.OnColorChanged?.Invoke(this, EventArgs.Empty);
                    }
                });
            }
        }

        #region "Statics (Sprites & Shared Resources)"

        private static TextureRegion2D[] spriteBoxes;
        private static TextureRegion2D spriteHighlight;

        private static void LoadStatics() {
            if (spriteBoxes != null) return;

            // Load static sprite regions
            spriteBoxes = new TextureRegion2D[] {
                Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v1"), Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v2"), Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v3"), Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v4"),
            };
            spriteHighlight = Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-active");
        }

        #endregion

        private readonly int _drawVariation;

        public ColorBox() : base() {
            LoadStatics();

            this.Size = new Point(COLOR_SIZE);

            _drawVariation = Utils.Calc.GetRandom(0, 3);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);

            this.BasicTooltipText = this.Color?.Id.ToString() ?? "None";
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(spriteBoxes[_drawVariation], bounds, this.Color?.Fur?.Rgb.ToXnaColor() ?? Microsoft.Xna.Framework.Color.White);

            if (this.MouseOver || this.Selected)
                spriteBatch.Draw(spriteHighlight, bounds, Microsoft.Xna.Framework.Color.White * 0.7f);
        }

    }

}
