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
    public class Checkbox:Label, ICheckable {

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

        private bool _checked = false;
        public bool Checked {
            get => _checked;
            set {
                if (_checked == value) return;

                _checked = value;
                OnPropertyChanged();

                OnCheckedChanged(new CheckChangedEvent(_checked));
            }
        }

        public Checkbox() : base() {
            LoadSprites();

            this.Height = CHECKBOX_SIZE / 2;

            LeftOffset = CHECKBOX_SIZE / 3 * 2;
            this.AutoSizeWidth = true;
            this.TextColor = Color.White;
            this.VerticalAlignment = Utils.DrawUtil.VerticalAlignment.Middle;
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

            spriteBatch.Draw(sprite, new Rectangle(-9, 0, CHECKBOX_SIZE, CHECKBOX_SIZE).OffsetBy(bounds.Location).OffsetBy(0, this.Height / 2 - CHECKBOX_SIZE / 2), Color.White);

            base.Paint(spriteBatch, bounds);
        }

    }
}
