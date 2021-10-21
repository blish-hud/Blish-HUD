using System;
using System.ComponentModel;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Blish_HUD._Extensions;

namespace Blish_HUD.Controls {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorBox : Control {

        public event EventHandler<EventArgs> ColorChanged;
        public event EventHandler<EventArgs> Selected;

        private const int    DEFAULT_COLOR_SIZE                          = 32;
        private const string COLOR_CHANGE_SOUND_NAME                     = "color-change";
        private const string DRAW_VARIATION_DYE_CHANNEL_NAME             = "colorpicker/cp-clr-dc";
        private const string DRAW_VARIATION_DYE_CHANNEL_X2_NAME          = "colorpicker/cp-clr-dc-x2";
        private const string DRAW_VARIATION_DYE_CHANNEL_X2_VERTICAL_NAME = "colorpicker/cp-clr-dc-x2-vert";
        private const string DRAW_VARIATION_DYE_CHANNEL_X4_NAME          = "colorpicker/cp-clr-dc-x4";
        private const string DRAW_VARIATION_VERSION_ONE_NAME             = "colorpicker/cp-clr-v1";
        private const string DRAW_VARIATION_VERSION_TWO_NAME             = "colorpicker/cp-clr-v2";
        private const string DRAW_VARIATION_VERSION_THREE_NAME           = "colorpicker/cp-clr-v3";
        private const string DRAW_VARIATION_VERSION_FOUR_NAME            = "colorpicker/cp-clr-v4";
        private const string HIGHLIGHT_NAME                              = "colorpicker/cp-clr-active";
        private const string HOVER_NAME                                  = "colorpicker/cp-clr-hover";

        private readonly int drawVariation;

        private bool isSelected = false;

        public bool IsSelected {
            get => isSelected;
            set {
                if (SetProperty(ref isSelected, value)) {
                    this.Selected?.Invoke(this, EventArgs.Empty);

                    if (this.Visible) Content.PlaySoundEffectByName(COLOR_CHANGE_SOUND_NAME);
                }
            }
        }

        private Gw2Sharp.WebApi.V2.Models.Color color;

        public Gw2Sharp.WebApi.V2.Models.Color Color {
            get => color;
            set {
                if (SetProperty(ref color, value)) {
                    this.ColorChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // Sprites used on the dye selection panel in game
        private static readonly TextureRegion2D[] _possibleDrawVariations = new TextureRegion2D[] {
            Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_VERSION_ONE_NAME), Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_VERSION_TWO_NAME),
            Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_VERSION_THREE_NAME), Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_VERSION_FOUR_NAME),
        };

        // Sprite when square with > 24 size, sprite used in the dye channel of armor in game
        private static readonly TextureRegion2D _spriteDyeChannel           = Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_DYE_CHANNEL_NAME);

        // Sprite when rectangle with more width than height, texture used in the dye channel of armor, appears when 1 < channels < 4
        private static readonly TextureRegion2D _spriteDyeChannelX2         = Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_DYE_CHANNEL_X2_NAME);
        private static readonly TextureRegion2D _spriteDyeChannelX2Vertical = Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_DYE_CHANNEL_X2_VERTICAL_NAME);

        // Sprite for when both width and height > 64, texture used in the dye channel of armor, for 1 channel armor
        private static readonly TextureRegion2D _spriteDyeChannelX4         = Resources.Control.TextureAtlasControl.GetRegion(DRAW_VARIATION_DYE_CHANNEL_X4_NAME);
        private static readonly TextureRegion2D _spriteHighlight            = Resources.Control.TextureAtlasControl.GetRegion(HIGHLIGHT_NAME);
        private static readonly TextureRegion2D _spriteHover                = Resources.Control.TextureAtlasControl.GetRegion(HOVER_NAME);
        
        public ColorBox() : base() {
            Size = new Point(DEFAULT_COLOR_SIZE);

            drawVariation = RandomUtil.GetRandom(0, _possibleDrawVariations.Length - 1);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);

            this.BasicTooltipText = this.Color?.Name ?? "None";
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            TextureRegion2D sprite = _possibleDrawVariations[drawVariation];
            if (this.Size.X == this.Size.Y && this.Size.X > 24 && this.Size.X < 64) {
                sprite = _spriteDyeChannel;
            } else if (this.Size.X > this.Size.Y) {
                sprite = _spriteDyeChannelX2;
            } else if (this.Size.X < this.Size.Y) {
                sprite = _spriteDyeChannelX2Vertical;
            } else if (this.Size.X >= 64) {
                sprite = _spriteDyeChannelX4;
            }

            spriteBatch.DrawOnCtrl(this, sprite, bounds, this.Color?.Cloth?.ToXnaColor() ?? Microsoft.Xna.Framework.Color.White);


            if (this.MouseOver) spriteBatch.DrawOnCtrl(this, _spriteHover, bounds, Microsoft.Xna.Framework.Color.White);
            if (this.IsSelected) spriteBatch.DrawOnCtrl(this, _spriteHighlight, bounds, Microsoft.Xna.Framework.Color.White);
        }

    }

}