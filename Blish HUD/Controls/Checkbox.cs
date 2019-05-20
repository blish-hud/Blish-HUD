using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {

    // TODO: Checkbox needs to shrink on mousedown (animation)
    public class Checkbox:LabelBase, ICheckable {

        public const int CHECKBOX_SIZE = 32;

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        protected virtual void OnCheckedChanged(CheckChangedEvent e) {
            this.CheckedChanged?.Invoke(this, e);
        }

        #region "Sprites"

        private static bool _spritesLoaded = false;

        private static List<TextureRegion2D> cbRegions;

        private static void LoadSprites() {
            if (_spritesLoaded) return;

            cbRegions = new List<TextureRegion2D>();

            cbRegions.AddRange(new TextureRegion2D[] {
                ControlAtlas.GetRegion("checkbox/cb-unchecked"),
                ControlAtlas.GetRegion("checkbox/cb-unchecked-active"),
                ControlAtlas.GetRegion("checkbox/cb-unchecked-disabled"),
                ControlAtlas.GetRegion("checkbox/cb-checked"),
                ControlAtlas.GetRegion("checkbox/cb-checked-active"),
                ControlAtlas.GetRegion("checkbox/cb-checked-disabled"),
            });            

            _spritesLoaded = true;
        }

        #endregion

        /// <summary>
        /// The text this <see cref="Checkbox"/> should show.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        private bool _checked = false;
        public bool Checked {
            get => _checked;
            set {
                if (SetProperty(ref _checked, value)) {
                    OnCheckedChanged(new CheckChangedEvent(_checked));
                }
            }
        }

        public Checkbox() : base() {
            LoadSprites();

            this.Height = CHECKBOX_SIZE / 2;

            _autoSizeWidth = true;
            _textColor = Color.White;
            _verticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle;
        }

        public override void RecalculateLayout() {
            base.RecalculateLayout();

            _size = new Point(CHECKBOX_SIZE / 3 * 2 + LabelRegion.X, _size.Y);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            base.OnLeftMouseButtonPressed(e);

            if (this.Enabled)
                this.Checked = !this.Checked;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            base.OnLeftMouseButtonReleased(e);

            if (this.Enabled)
                Content.PlaySoundEffectByName(@"audio\button-click");
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            string state = "-unchecked";
            state = this.Checked ? "-checked" : state;

            string extension = "";
            extension = this.MouseOver ? "-active" : extension;
            extension = !this.Enabled ? "-disabled" : extension;

            var sprite = cbRegions.First(cb => cb.Name == $"checkbox/cb{state}{extension}");

            spriteBatch.DrawOnCtrl(this,
                                   sprite,
                                   new Rectangle(-9,
                                                 this.Height / 2 - CHECKBOX_SIZE / 2,
                                                 CHECKBOX_SIZE,
                                                 CHECKBOX_SIZE));

            DrawText(spriteBatch, new Rectangle(CHECKBOX_SIZE / 3 * 2, 0, LabelRegion.X, LabelRegion.Y));
        }

    }
}
