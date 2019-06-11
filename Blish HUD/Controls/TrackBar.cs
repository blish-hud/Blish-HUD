using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Controls {
    public class TrackBar : Control {
        
        private const int BUFFER_WIDTH = 4;

        #region Load Static

        private static readonly TextureRegion2D _textureTrack;
        private static readonly TextureRegion2D _textureNub;

        static TrackBar() {
            _textureTrack = Resources.Control.TextureAtlasControl.GetRegion("trackbar/tb-track");
            _textureNub   = Resources.Control.TextureAtlasControl.GetRegion("trackbar/tb-nub");
        }

        #endregion

        public event EventHandler<EventArgs> ValueChanged;

        protected int _maxValue = 100;
        public int MaxValue {
            get => _maxValue;
            set => SetProperty(ref _maxValue, value, true);
        }

        protected int _minValue = 0;
        public int MinValue {
            get => _minValue;
            set => SetProperty(ref _minValue, value, true);
        }

        protected float _value = 50;
        public float Value {
            get => _value;
            set {
                if (SetProperty(ref _value, MathHelper.Clamp(value, _minValue, _maxValue), true)) {
                    this.ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int RoundedValue => (int)Math.Round(_value, 0);

        private bool Dragging = false;
        private int DraggingOffset = 0;

        public TrackBar() {
            this.Size = new Point(256, 16);

            this.LeftMouseButtonPressed += TrackBar_LeftMouseButtonPressed;
            Input.LeftMouseButtonReleased += Input_LeftMouseButtonReleased;
        }

        private void Input_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            Dragging = false;
        }

        private void TrackBar_LeftMouseButtonPressed(object sender, MouseEventArgs e) {
            if (_layoutNubBounds.Contains(this.RelativeMousePosition)) {
                Dragging = true;
                DraggingOffset = this.RelativeMousePosition.X - _layoutNubBounds.X;
            }
        }

        public override void DoUpdate(GameTime gameTime) {
            if (Dragging) {
                var relMousePos = this.RelativeMousePosition - new Point(DraggingOffset, 0);
                this.Value = ((float)relMousePos.X / (float)(this.Width - BUFFER_WIDTH * 2 - _textureNub.Width)) * (this.MaxValue - this.MinValue);
            }
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        #region Calculated Layout

        private Rectangle _layoutNubBounds;

        #endregion

        public override void RecalculateLayout() {
            float valueOffset = (((this.Value - this.MinValue) / (this.MaxValue - this.MinValue)) * (_textureTrack.Width - BUFFER_WIDTH * 2 - _textureNub.Width));
            _layoutNubBounds = new Rectangle((int)valueOffset + BUFFER_WIDTH, 0, _textureNub.Width, _textureNub.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, _textureTrack, bounds);
            
            spriteBatch.DrawOnCtrl(this, _textureNub, _layoutNubBounds);
        }

    }
}
