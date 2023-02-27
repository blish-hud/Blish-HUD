using System;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    public class ContextMenuStripItem : Control, ICheckable {

        private const int BULLET_SIZE        = 18;
        private const int HORIZONTAL_PADDING = 6;

        private const int TEXT_LEFTPADDING = HORIZONTAL_PADDING + BULLET_SIZE + HORIZONTAL_PADDING;

        #region Textures

        private readonly AsyncTexture2D _textureBullet = AsyncTexture2D.FromAssetId(155038);

        private static readonly Texture2D _textureArrow  = Content.GetTexture("context-menu-strip-submenu");

        #endregion

        public event EventHandler<CheckChangedEvent> CheckedChanged;
        protected virtual void OnCheckedChanged(CheckChangedEvent e) {
            this.CheckedChanged?.Invoke(this, e);
        }

        private string _text;
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        private ContextMenuStrip _submenu;
        public ContextMenuStrip Submenu {
            get => _submenu;
            set => SetProperty(ref _submenu, value, true);
        }

        private bool _canCheck = false;
        public bool CanCheck {
            get => _canCheck;
            set => SetProperty(ref _canCheck, value);
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
        
        public ContextMenuStripItem() {
            this.EffectBehind = new Effects.ScrollingHighlightEffect(this);
        }

        public ContextMenuStripItem(string itemText) : this() {
            this.Text = itemText;
        }

        public override void RecalculateLayout() {
            var textSize = GameService.Content.DefaultFont14.MeasureString(_text);
            int nWidth   = (int)textSize.Width + TEXT_LEFTPADDING + TEXT_LEFTPADDING;

            var parent = this.Parent;

            if (parent != null) {
                this.Width = Math.Max(parent.Width - 4, nWidth);
            } else {
                this.Width = nWidth;
            }
        }

        protected override void OnClick(MouseEventArgs e) {
            // It is important that we handle this first to avoid mistakenly removing
            // the handlers if the control is disposed of during the click.
            base.OnClick(e);

            if (this.CanCheck) {
                this.Checked = !this.Checked;
            } else {
                this.Parent?.Hide();
            }
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            if (this.Enabled) {
                this.Submenu?.Show(this);
            }

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            if (this.RelativeMousePosition.X < this.Left)
                this.Submenu?.Hide();

            base.OnMouseLeft(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var modifierTint = this.Enabled 
                                   ? this.MouseOver
                                        ? StandardColors.Tinted
                                        : StandardColors.Default
                                   : StandardColors.DisabledText;

            if (_canCheck) {
                string state = _checked ? "-checked" : "-unchecked";

                string extension = "";
                extension = this.MouseOver ? "-active" : extension;
                extension = !this.Enabled ? "-disabled" : extension;

                spriteBatch.DrawOnCtrl(this,
                                 Resources.Checkable.TextureRegionsCheckbox.First(cb => cb.Name == $"checkbox/cb{state}{extension}"),
                                 new Rectangle(HORIZONTAL_PADDING + BULLET_SIZE / 2 - 16,
                                               _size.Y / 2 - 16,
                                               32,
                                               32),
                                 StandardColors.Default);

            } else {
                spriteBatch.DrawOnCtrl(this,
                                 _textureBullet,
                                 new Rectangle(HORIZONTAL_PADDING,
                                               _size.Y / 2 - BULLET_SIZE / 2,
                                               BULLET_SIZE,
                                               BULLET_SIZE),
                                 modifierTint);
            }

            // Draw shadow
            spriteBatch.DrawStringOnCtrl(this,
                                           _text,
                                           Content.DefaultFont14,
                                           new Rectangle(TEXT_LEFTPADDING + 1,
                                                         0 + 1,
                                                         _size.X - TEXT_LEFTPADDING - HORIZONTAL_PADDING,
                                                         _size.Y),
                                           StandardColors.Shadow);

            spriteBatch.DrawStringOnCtrl(this,
                                           _text,
                                           Content.DefaultFont14,
                                           new Rectangle(TEXT_LEFTPADDING,
                                                         0,
                                                         _size.X - TEXT_LEFTPADDING - HORIZONTAL_PADDING,
                                                         _size.Y),
                                           _enabled ? StandardColors.Default : StandardColors.DisabledText);

            // Indicate submenu, if there is one
            if (_submenu != null) {
                spriteBatch.DrawOnCtrl(this,
                                 _textureArrow,
                                 new Rectangle(_size.X - HORIZONTAL_PADDING - _textureArrow.Width,
                                               _size.Y / 2 - _textureArrow.Height / 2,
                                               _textureArrow.Width,
                                               _textureArrow.Height),
                                 modifierTint);
            }
        }

    }

}