using System;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    // TODO: Checkbox needs to shrink on mousedown (animation)
    public class Checkbox : LabelBase, ICheckable {

        private const int CHECKBOX_SIZE = 32;

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        protected virtual void OnCheckedChanged(CheckChangedEvent e) {
            this.CheckedChanged?.Invoke(this, e);
        }

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
            _size = new Point(64, CHECKBOX_SIZE / 2);

            _autoSizeWidth     = true;
            _textColor         = Color.White;
            _verticalAlignment = VerticalAlignment.Middle;
        }

        public override void RecalculateLayout() {
            base.RecalculateLayout();

            _size = new Point(CHECKBOX_SIZE / 3 * 2 + LabelRegion.X, _size.Y);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            if (this.Enabled)
                this.Checked = !this.Checked;

            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            if (this.Enabled)
                Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnLeftMouseButtonReleased(e);
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

            var sprite = Resources.Checkable.TextureRegionsCheckbox.First(cb => cb.Name == $"checkbox/cb{state}{extension}");

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
