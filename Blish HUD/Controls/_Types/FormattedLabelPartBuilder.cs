using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class FormattedLabelPartBuilder {
        private readonly string _text;
        private bool _isBold;
        private bool _isItalic;
        private bool _isStrikeThrough;
        private bool _isUnderlined;
        private Action _link;
        private Texture2D _prefixImage;
        private Texture2D _suffixImage;
        private Color _textColor;
        private Color _hoverColor = Color.LightBlue;
        private ContentService.FontSize _fontSize = ContentService.FontSize.Size18;

        internal FormattedLabelPartBuilder(string text) {
            _text = text;
        }

        public FormattedLabelPartBuilder MakeBold() {
            _isItalic = false;
            _isBold = true;
            return this;
        }

        public FormattedLabelPartBuilder MakeItalic() {
            _isBold = false;
            _isItalic = true;
            return this;
        }

        public FormattedLabelPartBuilder MakeStrikeThrough() {
            _isStrikeThrough = true;
            return this;
        }

        public FormattedLabelPartBuilder MakeUnderlined() {
            _isUnderlined = true;
            return this;
        }

        public FormattedLabelPartBuilder SetLink(Action onLink) {
            _link = onLink;
            return this;
        }

        public FormattedLabelPartBuilder SetHyperLink(string link) {
            _link = new Action(() => System.Diagnostics.Process.Start(link));
            return this;
        }

        public FormattedLabelPartBuilder SetPrefixImage(Texture2D prefixImage) {
            _prefixImage = prefixImage;
            return this;
        }

        public FormattedLabelPartBuilder SetSuffixImage(Texture2D suffixImage) {
            _suffixImage = suffixImage;
            return this;
        }

        public FormattedLabelPartBuilder SetTextColor(Color textColor) {
            _textColor = textColor;
            return this;
        }

        public FormattedLabelPartBuilder SetHoverColor(Color hoverColor) {
            _hoverColor = hoverColor;
            return this;
        }

        public FormattedLabelPartBuilder SetFontSize(ContentService.FontSize fontSize) {
            _fontSize = fontSize;
            return this;
        }

        internal FormattedLabelPart Build()
            => new FormattedLabelPart(
                _isBold,
                _isItalic,
                _isStrikeThrough,
                _isUnderlined,
                _text,
                _link,
                _prefixImage,
                _suffixImage,
                _textColor,
                _hoverColor,
                _fontSize,
                ContentService.FontFace.Menomonia);
    }
}