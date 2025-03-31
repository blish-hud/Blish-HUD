using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;

namespace Blish_HUD.Controls {
    internal class FormattedLabelPart : IDisposable {
        public BitmapFont Font { get; }

        public bool IsBold { get; }

        public bool IsItalic { get; }

        public bool IsStrikeThrough { get; }

        public bool IsUnderlined { get; }

        public string Text { get; }

        public Action Link { get; }

        public AsyncTexture2D PrefixImage { get; }

        public AsyncTexture2D SuffixImage { get; }

        public Point PrefixImageSize { get; }

        public Point SuffixImageSize { get; }

        public ContentService.FontSize FontSize { get; }

        public ContentService.FontFace FontFace { get; }

        public Color TextColor { get; }

        public Color HoverColor { get; }

        public FormattedLabelPart(
            bool isBold,
            bool isItalic,
            bool isStrikeThrough,
            bool isUnderlined,
            string text,
            Action link,
            AsyncTexture2D prefixImage,
            AsyncTexture2D suffixImage,
            Point prefixImageSize,
            Point suffixImageSize,
            Color textColor,
            Color hoverColor,
            ContentService.FontSize fontSize,
            ContentService.FontFace fontFace) {
            this.IsBold = isBold;
            this.IsItalic = isItalic;
            this.IsStrikeThrough = isStrikeThrough;
            this.IsUnderlined = isUnderlined;
            this.Text = text;
            this.Link = link;
            this.PrefixImage = prefixImage;
            this.SuffixImage = suffixImage;
            this.PrefixImageSize = prefixImageSize;
            this.SuffixImageSize = suffixImageSize;
            this.HoverColor = hoverColor;
            this.FontSize = fontSize;
            this.FontFace = fontFace;
            this.TextColor = textColor == default ? Color.White : textColor;

            var style = ContentService.FontStyle.Regular;

            if (this.IsItalic) {
                style = ContentService.FontStyle.Italic;
            } else if (this.IsBold) {
                style = ContentService.FontStyle.Bold;
            }

            this.Font = GameService.Content.GetFont(this.FontFace, this.FontSize, style);
        }

        public void Dispose() {
            this.PrefixImage?.Dispose();
            this.SuffixImage?.Dispose();
        }
    }
}