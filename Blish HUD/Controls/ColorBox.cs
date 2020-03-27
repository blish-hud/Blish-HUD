using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Blish_HUD._Extensions;

namespace Blish_HUD.Controls {

    // TODO: Need to have events updated in ColorBox to match the standard applied in Control class
    // TODO: Need to revisit the implementation of ColorBox
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorBox : Control {

        public event EventHandler<EventArgs> OnColorChanged;
        public event EventHandler<EventArgs> OnSelected;

        private const int ColorSize = 32;
        private const string ColorChangeSoundName = @"audio\color-change";
        private const string DrawVariationVersionOneName = "colorpicker/cp-clr-v1";
        private const string DrawVariationVersionTwoName = "colorpicker/cp-clr-v2";
        private const string DrawVariationVersionThreeName = "colorpicker/cp-clr-v3";
        private const string DrawVariationVersionFourName = "colorpicker/cp-clr-v4";
        private const string HighlightName = "colorpicker/cp-clr-active";
        private readonly int drawVariation;

        private bool selected = false;

        public bool Selected {
            get => selected;
            set {
                if (SetProperty(ref selected, value)) {
                    this.OnSelected?.Invoke(this, EventArgs.Empty);

                    if (this.Visible)
                        Content.PlaySoundEffectByName(ColorChangeSoundName);
                }
            }
        }

        private Gw2Sharp.WebApi.V2.Models.Color color;

        public Gw2Sharp.WebApi.V2.Models.Color Color {
            get => color;
            set {
                if (color == value)
                    return;

                color = value;

                OnPropertyChanged(nameof(this.ColorId));
                OnPropertyChanged();
                this.OnColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ColorId => this.Color?.Id ?? -1;

        private static TextureRegion2D[] _possibleDrawVariations;
        private static TextureRegion2D _spriteHighlight;

        private static void LoadStatics() {
            if (_possibleDrawVariations != null)
                return;

            // Load static sprite regions
            _possibleDrawVariations = new TextureRegion2D[] {
                Resources.Control.TextureAtlasControl.GetRegion(DrawVariationVersionOneName),
                Resources.Control.TextureAtlasControl.GetRegion(DrawVariationVersionTwoName), 
                Resources.Control.TextureAtlasControl.GetRegion(DrawVariationVersionThreeName),
                Resources.Control.TextureAtlasControl.GetRegion(DrawVariationVersionFourName),
            };

            _spriteHighlight = Resources.Control.TextureAtlasControl.GetRegion(HighlightName);
        }


        public ColorBox() : base() {
            LoadStatics();

            Size = new Point(ColorSize);

            drawVariation = RandomUtil.GetRandom(0, _possibleDrawVariations.Length - 1);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);

            this.BasicTooltipText = this.Color?.Name ?? "None";
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _possibleDrawVariations[drawVariation], bounds, this.Color?.Fur?.ToXnaColor() ?? Microsoft.Xna.Framework.Color.White);

            if (this.MouseOver || this.Selected)
                spriteBatch.DrawOnCtrl(this, _spriteHighlight, bounds, Microsoft.Xna.Framework.Color.White * 0.7f);
        }

    }

}
