using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Utils {
    public static class String {

        public static string FilterByFont(SpriteFont font, string text) {
            var cleanText = new StringBuilder();

            foreach (char c in text) {
                if (font.Characters.Contains(c)) cleanText.Append(c);
            }

            return cleanText.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <remarks>Adapted from https://stackoverflow.com/a/31514114/595437 (removed yields and handles adding newlines, builtin)</remarks>
        public static string SplitText(string text, int length) {
            var brokenString = new string[(int)Math.Ceiling((double)text.Length / length)];

            int line, i;
            for (line = i = 0; i < text.Length; i += length) {
                brokenString[line] = text.Substring(i, Math.Min(length, text.Length - i));
                line++;
            }

            return string.Join(Environment.NewLine, brokenString);
        }

        public static int ComputeLevenshteinDistance(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];
            
            if (n == 0) {
                return m;
            }

            if (m == 0) {
                return n;
            }
            
            for (int i = 0; i <= n; d[i, 0] = i++) {
            }

            for (int j = 0; j <= m; d[0, j] = j++) {
            }
            
            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

    }
}
