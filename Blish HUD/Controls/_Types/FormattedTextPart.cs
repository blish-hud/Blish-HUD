using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    internal class FormattedTextPart {
        public BitmapFont Font { get; }

        public bool IsBold { get; }

        public bool IsItalic { get; }

        public bool IsStrikeThrough { get; }

        public bool IsUnderlined { get; }

        public string Text { get; }

        public Action Link { get; }

        public Texture2D PrefixImage { get; }
        
        public ContentService.FontSize FontSize { get; }
        
        public ContentService.FontFace FontFace { get; }
        
        public Color TextColor { get; }

        public Color HoverColor { get; }

        public FormattedTextPart(
            bool isBold,
            bool isItalic,
            bool isStrikeThrough,
            bool isUnderlined,
            string text,
            Action link,
            Texture2D prefixImage,
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