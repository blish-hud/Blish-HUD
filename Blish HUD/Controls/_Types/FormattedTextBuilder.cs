using System;
using System.Collections.Generic;

namespace Blish_HUD.Controls {
    public class FormattedTextBuilder {
        private readonly List<FormattedTextPart> _parts = new List<FormattedTextPart>();
        private bool _wrapText;
        private int _width;
        private int _height;
        private bool _autoSizeHeight;
        private bool _autoSizeWidth;
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Middle;

        public FormattedTextBuilder CreatePart(string text, Action<FormattedTextPartBuilder> creationFunc) {
            var builder = new FormattedTextPartBuilder(text);
            creationFunc?.Invoke(builder);
            _parts.Add(builder.Build());
            return this;
        }

        public FormattedTextBuilder Wrap() {
            _wrapText = true;
            return this;
        }

        public FormattedTextBuilder SetWidth(int width) {
            _autoSizeWidth = false;
            _width = width;
            return this;
        }

        public FormattedTextBuilder SetHeight(int height) {
            _autoSizeHeight = false;
            _height = height;
            return this;
        }

        public FormattedTextBuilder AutoSizeWidth() {
            _width = default;
            _autoSizeWidth = true;
            return this;
        }

        public FormattedTextBuilder AutoSizeHeight() {
            _height = default;
            _autoSizeHeight = true;
            return this;
        }

        public FormattedTextBuilder SetHorizontalAlignment(HorizontalAlignment horizontalAlignment) {
            _horizontalAlignment = horizontalAlignment;
            return this;
        }

        public FormattedTextBuilder SetVerticalAlignment(VerticalAlignment verticalAlignment) {
            _verticalAlignment = verticalAlignment;
            return this;
        }

        public FormattedText Build()
            => new FormattedText(_parts, _wrapText, _autoSizeWidth, _autoSizeHeight, _horizontalAlignment, _verticalAlignment) {
                Width = _width,
                Height = _height,
            };
    }
}