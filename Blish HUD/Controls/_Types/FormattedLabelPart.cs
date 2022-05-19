using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    internal class FormattedLabelPart {
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
            IsBold = isBold;
            IsItalic = isItalic;
            IsStrikeThrough = isStrikeThrough;
            IsUnderlined = isUnderlined;
            Text = text;
            Link = link;
            PrefixImage = prefixImage;
            SuffixImage = suffixImage;
            PrefixImageSize = prefixImageSize;
            SuffixImageSize = suffixImageSize;
            HoverColor = hoverColor;
            FontSize = fontSize;
            FontFace = fontFace;
            TextColor = textColor == default ? Color.White : textColor;

            var style = ContentService.FontStyle.Regular;

            if (IsItalic) {
                style = ContentService.FontStyle.Italic;
            } else if (IsBold) {
                style = ContentService.FontStyle.Bold;
            }

            Font = GameService.Content.GetFont(FontFace, FontSize, style);
        }
    }
}