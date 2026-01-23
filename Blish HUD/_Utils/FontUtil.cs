using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace Blish_HUD {
    [Flags]
    public enum Gw2FontRanges {
        None = 0,

        BasicLatin            = 1 << 0,
        Latin1Supplement      = 1 << 1,
        LatinExtendedA        = 1 << 2,
        CurrencySymbols       = 1 << 3,
        Arrows                = 1 << 4,
        MathOperators         = 1 << 5,
        EnclosedAlphanumerics = 1 << 6,
        BoxDrawing            = 1 << 7,
        GeometricShapes       = 1 << 8,
        DigitsOnly            = 1 << 9,
        Default = 
            BasicLatin       |
            Latin1Supplement |
            LatinExtendedA,
        All =
            BasicLatin            |
            Latin1Supplement      |
            LatinExtendedA        |
            CurrencySymbols       |
            Arrows                |
            MathOperators         |
            EnclosedAlphanumerics |
            BoxDrawing            |
            GeometricShapes
    }

    internal static class FontUtil {
        public static IReadOnlyList<CharacterRange> GetRanges(Gw2FontRanges ranges) {
            var list = new List<CharacterRange>();

            if (ranges.HasFlag(Gw2FontRanges.BasicLatin)) {
                list.Add(CharacterRange.BasicLatin);
            }

            if (ranges.HasFlag(Gw2FontRanges.Latin1Supplement)) {
                list.Add(CharacterRange.Latin1Supplement);
            }

            if (ranges.HasFlag(Gw2FontRanges.LatinExtendedA)) {
                list.Add(CharacterRange.LatinExtendedA);
            }
            if (ranges.HasFlag(Gw2FontRanges.CurrencySymbols)) {
                list.Add(new CharacterRange('₣', '₾'));
            }

            if (ranges.HasFlag(Gw2FontRanges.Arrows)) {
                list.Add(new CharacterRange('←', '⇿'));
            }

            if (ranges.HasFlag(Gw2FontRanges.MathOperators)) {
                list.Add(new CharacterRange('∀', '⋿'));
            }

            if (ranges.HasFlag(Gw2FontRanges.EnclosedAlphanumerics)) {
                list.Add(new CharacterRange('①', '⓿'));
            }

            if (ranges.HasFlag(Gw2FontRanges.BoxDrawing)) {
                list.Add(new CharacterRange('─', '╿'));
            }

            if (ranges.HasFlag(Gw2FontRanges.GeometricShapes)) {
                list.Add(new CharacterRange('■', '◿'));
            }

            if (ranges.HasFlag(Gw2FontRanges.DigitsOnly)) {
                list.Add(new CharacterRange('0', '9'));
                list.Add(new CharacterRange(':', ':'));
                list.Add(new CharacterRange('.', '.'));
                list.Add(new CharacterRange(',', ','));
                list.Add(new CharacterRange('-', '-'));
                list.Add(new CharacterRange('+', '+'));
                list.Add(new CharacterRange(' ', ' '));
                list.Add(new CharacterRange('%', '%'));
            }

            // Cause crash with only windows shipped .NET Runtime.
            //new CharacterRange('\u2000', '\u206F'), // General Punctuation
            //new CharacterRange('℀', '⅏'), // Letterlike Symbols
            //new CharacterRange('⌀', '⏺'), // Miscellaneous Technical
            //new CharacterRange('☀', '♪'), // Miscellaneous Symbols
            return list;
        }
    }
}
