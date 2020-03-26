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

namespace Blish_HUD.Controls
{

    // TODO: Need to have events updated in ColorBox to match the standard applied in Control class
    // TODO: Need to revisit the implementation of ColorBox
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorBox : Control
    {

        public event EventHandler<EventArgs> OnColorChanged;
        public event EventHandler<EventArgs> OnSelected;

        private const int COLOR_SIZE = 32;

        private bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    this.OnSelected?.Invoke(this, EventArgs.Empty);
                    if (this.Visible)
                        Content.PlaySoundEffectByName(@"audio\color-change");
                }
            }
        }

        private Gw2Sharp.WebApi.V2.Models.Color _color;

        public Gw2Sharp.WebApi.V2.Models.Color Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;

                _color = value;
                _colorId = value?.Id ?? -1;

                OnPropertyChanged("ColorId");
                OnPropertyChanged();
                this.OnColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private static Gw2Sharp.WebApi.V2.Models.Color[] Colors = new Gw2Sharp.WebApi.V2.Models.Color[]
        {
            new Gw2Sharp.WebApi.V2.Models.Color()
            {
                Id =  10,
                Name = "Sky",
                BaseRgb = new List<int>() {128, 26, 26},
                Cloth = new ColorMaterial()
                {
                    Brightness = 22,
                    Contrast = 1.25,
                    Hue = 196,
                    Saturation = 0.742188,
                    Lightness = 1.32813,
                    Rgb = new List<int>() {91, 123, 146}
                },
                Leather = new ColorMaterial()
                {
                    Brightness = 22,
                    Contrast = 1.25,
                    Hue = 196,
                    Saturation = 0.664063,
                    Lightness = 1.32813,
                    Rgb = new List<int>() {61, 123, 146}
                },
                Metal = new ColorMaterial()
                {
                    Brightness = 22,
                    Contrast = 1.28906,
                    Hue = 196,
                    Saturation = 0.546875,
                    Lightness = 1.32813,
                    Rgb = new List<int>() {65, 123, 146}
                },
                Fur = new ColorMaterial()
                {
                    Brightness = 22,
                    Contrast = 1.25,
                    Hue = 196,
                    Saturation = 0.742188,
                    Lightness = 1.32813,
                    Rgb = new List<int>() {54, 123, 146}
                },
                Item = 20370,
                Categories = new List<string>() {"Blue", "Vibrant", "Rare"}
            }
        };

        private int _colorId;
        public int ColorId
        {
            get { return _colorId; }
            set
            {
                if (_colorId == value)
                    return;

                _colorId = value;
                _color = Colors.FirstOrDefault(x => x.Id == this.ColorId);

                OnPropertyChanged();
                OnPropertyChanged("Color");
                this.OnColorChanged?.Invoke(this, EventArgs.Empty);

            }
        }

        #region "Statics (Sprites & Shared Resources)"

        private static TextureRegion2D[] spriteBoxes;
        private static TextureRegion2D spriteHighlight;

        private static void LoadStatics()
        {
            if (spriteBoxes != null)
                return;

            // Load static sprite regions
            spriteBoxes = new TextureRegion2D[] {
                Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v1"), Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v2"), Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v3"), Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-v4"),
            };
            spriteHighlight = Resources.Control.TextureAtlasControl.GetRegion("colorpicker/cp-clr-active");
        }

        #endregion

        private readonly int _drawVariation;

        public ColorBox() : base()
        {
            LoadStatics();

            this.Size = new Point(COLOR_SIZE);

            _drawVariation = RandomUtil.GetRandom(0, 3);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            this.BasicTooltipText = this.Color?.Id.ToString() ?? "None";
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, spriteBoxes[_drawVariation], bounds, this.Color?.Fur?.ToXnaColor() ?? Microsoft.Xna.Framework.Color.White);

            if (this.MouseOver || this.Selected)
                spriteBatch.DrawOnCtrl(this, spriteHighlight, bounds, Microsoft.Xna.Framework.Color.White * 0.7f);
        }

    }

}
