using System;
using System.Collections.Generic;

namespace Blish_HUD.Controls {
    public class FormattedLabelBuilder {
        private readonly List<FormattedLabelPart> _parts = new List<FormattedLabelPart>();
        private bool _wrapText;
        private int _width;
        private int _height;
        private bool _autoSizeHeight;
        private bool _autoSizeWidth;
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Middle;

        public FormattedLabelPartBuilder CreatePart(string text)
            => new FormattedLabelPartBuilder(text);

        public FormattedLabelBuilder CreatePart(string text, Action<FormattedLabelPartBuilder> creationFunc = null) {
            var builder = new FormattedLabelPartBuilder(text);
            creationFunc?.Invoke(builder);
            _parts.Add(builder.Build());
            return this;
        }

        public FormattedLabelBuilder CreatePart(FormattedLabelPartBuilder builder) {
            _parts.Add(builder.Build());
            return this;
        }

        public FormattedLabelBuilder Wrap() {
            _wrapText = true;
            return this;
        }

        public FormattedLabelBuilder SetWidth(int width) {
            _autoSizeWidth = false;
            _width = width;
            return this;
        }

        public FormattedLabelBuilder SetHeight(int height) {
            _autoSizeHeight = false;
            _height = height;
            return this;
        }

        public FormattedLabelBuilder AutoSizeWidth() {
            _width = default;
            _autoSizeWidth = true;
            return this;
        }

        public FormattedLabelBuilder AutoSizeHeight() {
            _height = default;
            _autoSizeHeight = true;
            return this;
        }

        public FormattedLabelBuilder SetHorizontalAlignment(HorizontalAlignment horizontalAlignment) {
            _horizontalAlignment = horizontalAlignment;
            return this;
        }

        public FormattedLabelBuilder SetVerticalAlignment(VerticalAlignment verticalAlignment) {
            _verticalAlignment = verticalAlignment;
            return this;
        }

        public FormattedLabel Build()
            => new FormattedLabel(_parts, _wrapText, _autoSizeWidth, _autoSizeHeight, _horizontalAlignment, _verticalAlignment) {
                Width = _width,
                Height = _height,
            };
    }
}