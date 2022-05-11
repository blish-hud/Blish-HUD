using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class FormattedTextPartBuilder {
        private readonly string _text;
        private bool _isBold;
        private bool _isItalic;
        private bool _isStrikeThrough;
        private bool _isUnderlined;
        private Action _link;
        private Texture2D _prefixImage;
        private Color _textColor;
        private Color _hoverColor = Color.LightBlue;
        private ContentService.FontSize _fontSize = ContentService.FontSize.Size18;

        internal FormattedTextPartBuilder(string text) {
            _text = text;
        }

        public FormattedTextPartBuilder MakeBold() {
            _isItalic = false;
            _isBold = true;
            return this;
        }

        public FormattedTextPartBuilder MakeItalic() {
            _isBold = false;
            _isItalic = true;
            return this;
        }

        public FormattedTextPartBuilder MakeStrikeThrough() {
            _isStrikeThrough = true;
            return this;
        }

        public FormattedTextPartBuilder MakeUnderlined() {
            _isUnderlined = true;
            return this;
        }

        public FormattedTextPartBuilder SetLink(Action onLink) {
            _link = onLink;
            return this;
        }

        public FormattedTextPartBuilder SetHyperLink(string link) {
            _link = new Action(() => System.Diagnostics.Process.Start(link));
            return this;
        }

        public FormattedTextPartBuilder SetPrefixImage(Texture2D prefixImage) {
            _prefixImage = prefixImage;
            return this;
        }

        public FormattedTextPartBuilder SetTextColor(Color textColor) {
            _textColor = textColor;
            return this;
        }

        public FormattedTextPartBuilder SetHoverColor(Color hoverColor) {
            _hoverColor = hoverColor;
            return this;
        }

        public FormattedTextPartBuilder SetFontSize(ContentService.FontSize fontSize) {
            _fontSize = fontSize;
            return this;
        }

        internal FormattedTextPart Build()
            => new FormattedTextPart(
                _isBold,
                _isItalic,
                _isStrikeThrough,
                _isUnderlined,
                _text,
                _link,
                _prefixImage,
                _textColor,
                _hoverColor,
                _fontSize,
                ContentService.FontFace.Menomonia);
    }
}