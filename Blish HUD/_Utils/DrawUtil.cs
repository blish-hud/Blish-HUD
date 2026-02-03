using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Linq;
using System.Text;
using Blish_HUD.Controls;
using System.Collections.Generic;

namespace Blish_HUD {
    public static class DrawUtil {

        public static void DrawAlignedText(SpriteBatch sb, SpriteFont sf, string text, Rectangle bounds, Color clr, HorizontalAlignment ha, VerticalAlignment va) {
            // Filter out any characters our font doesn't support
            text = string.Join("", text.ToCharArray().Where(c => sf.Characters.Contains(c)));

            var textSize = sf.MeasureString(text);

            int xPos = bounds.X;
            int yPos = bounds.Y;

            if (ha == HorizontalAlignment.Center) xPos += bounds.Width / 2 - (int)textSize.X / 2;
            if (ha == HorizontalAlignment.Right) xPos += bounds.Width - (int)textSize.X;

            if (va == VerticalAlignment.Middle) yPos += bounds.Height / 2 - (int)textSize.Y / 2;
            if (va == VerticalAlignment.Bottom) yPos += bounds.Height - (int)textSize.Y;

            sb.DrawString(sf, text, new Vector2(xPos, yPos), clr);
        }

        public static void DrawAlignedText(SpriteBatch sb, BitmapFont sf, string text, Rectangle bounds, Color clr, HorizontalAlignment ha = HorizontalAlignment.Left, VerticalAlignment va = VerticalAlignment.Middle) {
            Vector2 textSize = sf.MeasureString(text);

            int xPos = bounds.X;
            int yPos = bounds.Y;

            if (ha == HorizontalAlignment.Center) xPos += bounds.Width / 2 - (int)textSize.X / 2;
            if (ha == HorizontalAlignment.Right) xPos += bounds.Width - (int)textSize.X;

            if (va == VerticalAlignment.Middle) yPos += bounds.Height / 2 - (int)textSize.Y / 2;
            if (va == VerticalAlignment.Bottom) yPos += bounds.Height - (int)textSize.Y;

            sb.DrawString(sf, text, new Vector2(xPos, yPos), clr);
        }

        /// <summary>
        /// Wraps a <paramref name="word"/>, if it does not fit into the <paramref name="maxLineWidth"/>
        /// accounting for the given <paramref name="offset"/>.
        /// </summary>
        /// <remarks>
        /// Will prioritize wrapping a word at any of the given <paramref name="preferredWrapCharacters"/>,
        /// but will wrap in the middle of the word if none of them occur.
        /// </remarks>
        /// <param name="spriteFont"></param>
        /// <param name="word"></param>
        /// <param name="offset"></param>
        /// <param name="maxLineWidth"></param>
        /// <param name="preferredWrapCharacters"></param>
        /// <param name="newLineIndices"></param>
        /// <returns>The <paramref name="word"/> with new line characters at appropriate
        /// positions to make it fit into the <paramref name="maxLineWidth"/>.</returns>
        private static string WrapWord(BitmapFont spriteFont, string word, float offset, float maxLineWidth, char[] preferredWrapCharacters, out int[] newLineIndices) {
            newLineIndices = Array.Empty<int>();
            if (string.IsNullOrEmpty(word)) return string.Empty;

            if (offset + spriteFont.MeasureString(word).Width <= maxLineWidth) return word;

            StringBuilder resultBuilder = new StringBuilder();

            List<int> indices = new List<int>();
            bool didSplitCharacterOccur = false;

            StringBuilder partBuilder = new StringBuilder();

            // this is neccessary, because measuring each character individually and
            // adding them up, results in a significant higher value that measuring the whole line
            float currentLineWithNewCharacterWidth;
            string currentLineCharacters = string.Empty;

            for (int i = 0; i < word.Length; i++) {
                if (indices.Any()) {
                    offset = 0;
                }

                char character = word[i];
                currentLineCharacters += character;
                currentLineWithNewCharacterWidth = spriteFont.MeasureString(currentLineCharacters).Width + offset;

                if (currentLineWithNewCharacterWidth < maxLineWidth) {
                    partBuilder.Append(character);
                    
                    if (preferredWrapCharacters.Contains(character)) {
                        resultBuilder.Append(partBuilder);
                        partBuilder.Clear();
                        didSplitCharacterOccur = true;
                    }
                } else {

                    int characterOffset = 0;

                    if (!didSplitCharacterOccur) {
                        resultBuilder.Append(partBuilder);
                        resultBuilder.Append('\n');
                        currentLineCharacters = string.Empty;
                    }
                    else {
                        resultBuilder.Append('\n');
                        resultBuilder.Append(partBuilder);
                        currentLineCharacters = partBuilder.ToString(); 

                        characterOffset = partBuilder.Length;
                    }

                    indices.Add(i + indices.Count() - characterOffset);

                    partBuilder.Clear();
                    partBuilder.Append(character);
                    currentLineCharacters += character;

                    didSplitCharacterOccur = false;
                }
            }

            if (partBuilder.Length != 0) {
                resultBuilder.Append(partBuilder);
            }

            newLineIndices = indices.ToArray();
            return resultBuilder.ToString();
        }

        private static string WrapTextSegment(BitmapFont spriteFont, string text, float maxLineWidth) {
            return WrapTextSegment(spriteFont, text, maxLineWidth, out _);
        }

        private static string WrapTextSegment(BitmapFont spriteFont, string text, float maxLineWidth, out int[] newLineIndices) {
            return WrapTextSegment(spriteFont, text, maxLineWidth, Array.Empty<char>(), out newLineIndices);
        }

        /// <remarks>
        /// Original source: https://stackoverflow.com/a/15987581/595437
        /// (modified)
        /// </remarks>
        private static string WrapTextSegment(BitmapFont spriteFont, string text, float maxLineWidth, char[] preferredWrapCharacters, out int[] newLineIndices) {
            newLineIndices = Array.Empty<int>();
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string[] words = text.Split(' ');
            var sb = new StringBuilder();
            float currentLineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").Width;

            List<int> indices = new List<int>();
            int processedCharacters = 0;

            for (int i = 0; i < words.Length; i++) {
                string word = words[i];
                float wordWidth = spriteFont.MeasureString(word).Width;

                if (currentLineWidth + wordWidth < maxLineWidth) {
                    sb.Append(word);
                    currentLineWidth += wordWidth;
                    if (i < words.Length - 1) {
                        sb.Append(" ");
                        currentLineWidth += spaceWidth;
                        processedCharacters++;
                    }
                } else {
                    string wrappedWord = WrapWord(spriteFont, word, currentLineWidth, maxLineWidth, preferredWrapCharacters, out int[] wordNewLineIndices);

                    string firstPart = wrappedWord;
                    string lastPart = wrappedWord;

                    if (wordNewLineIndices.Length != 0) {
                        firstPart = wrappedWord.Substring(0, wordNewLineIndices.First());
                        lastPart = wrappedWord.Substring(wordNewLineIndices.Last() + 1);
                    }

                    // words should only every be broken in the middle of the word (no wrap character
                    // in the first part), if they started on their own line.
                    if (preferredWrapCharacters.Any(character => firstPart.Contains(character)) || currentLineWidth == 0) {
                        sb.Append(wrappedWord);
                        currentLineWidth = spriteFont.MeasureString(lastPart).Width;

                        int indexOffset = processedCharacters + indices.Count();

                        foreach (int wordIndex in wordNewLineIndices) {
                            indices.Add(wordIndex + indexOffset);
                        }
                    } else {
                        string wrappedWordOnNextLine = WrapWord(spriteFont, word, 0, maxLineWidth, preferredWrapCharacters, out int[] wordOnNextLineNewLineIndices);
                        sb.Append('\n');
                        indices.Add(processedCharacters + indices.Count());
                        sb.Append(wrappedWordOnNextLine);
                        currentLineWidth = spriteFont.MeasureString(wrappedWordOnNextLine.Split('\n').Last()).Width;

                        int indexOffset = processedCharacters + indices.Count();

                        foreach (int wordIndex in wordOnNextLineNewLineIndices) {
                            indices.Add(wordIndex + indexOffset);
                        }
                    }

                    if (i < words.Length - 1) {
                        sb.Append(" ");
                        processedCharacters++;
                        currentLineWidth += spaceWidth;
                    }
                }
                processedCharacters += word.Length;
            }

            newLineIndices = indices.ToArray();
            return sb.ToString();
        }

        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth) {
            return WrapText(spriteFont, text, maxLineWidth, out _);
        }

        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth, out int[] newLineIndices) {
            return WrapText(spriteFont, text, maxLineWidth, Array.Empty<char>(), out newLineIndices);
        }

        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth, char[] preferredWrapCharacters, out int[] newLineIndices) {
            newLineIndices = Array.Empty<int>();
            if (string.IsNullOrEmpty(text)) return "";

            var sb = new StringBuilder();
            List<int> indices = new List<int>();
            int processedCharacters = 0;

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++) {
                sb.Append(WrapTextSegment(spriteFont, lines[i], maxLineWidth, preferredWrapCharacters, out int[] segmentNewLineIndices));

                int indexOffset = processedCharacters + indices.Count();

                foreach (int segmentIndex in segmentNewLineIndices) {
                    indices.Add(segmentIndex + indexOffset);
                }

                processedCharacters += lines[i].Length;

                if (i < lines.Length - 1) {
                    sb.Append('\n');
                    processedCharacters++;
                }
            }

            newLineIndices = indices.ToArray();
            return sb.ToString();
        }
    }
}
