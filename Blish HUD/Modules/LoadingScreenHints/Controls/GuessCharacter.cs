using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Text.RegularExpressions;
namespace Blish_HUD.Modules.LoadingScreenHints.Controls {
    public class GuessCharacter : Control {
        public static List<string> Characters = new List<string>
        {
            "Blish",
            "Taimi",
            "Braham Eirsson",
            "Rox Whetstone",
            "Kasmeer Meade",
            "Marjory Delaqua",
            "Blish",
            "Gorrik",
            "Logan Thackeray",
            "Zojja",
            "Rytlock Brimstone",
            "Snaff",
            "Eir Stegalkin",
            "Caithe",
            "the Avatar of The Pale Tree",
            "Trahearne",
            "Canach",
            "Koss",
            "Palawa Ignacious Joko",
            "Scruffy",
            "Countess Anise",
            "Queen Jennah",
            "Almorra Soulkeeper",
            "Scarlet Briar",
            "Riel Darkwater",
            "Turai Ossa",
            "Forgal Kernsson",
            "Steward Gixx",
            "Faolain",
            "Lord Faren",
            /*"",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",*/
        };
        private Texture2D CharacterTexture;
        private string CharacterString;
        public Image CharacterImage;
        private Effect SilhouetteFX = Overlay.cm.Load<Effect>(@"effects\silhouette");
        private Effect GlowFX = Overlay.cm.Load<Effect>(@"effects\glow");
        private bool _result;
        public bool Result
        {
            get => _result;
            set
            {
                if (value == _result) return;
                CharacterImage.SpriteBatchParameters.Effect = value ? GlowFX : SilhouetteFX;
                CharacterImage.Invalidate();
                _result = value;
                this.OnPropertyChanged();
            }
        }
        private BitmapFont Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        public GuessCharacter(int rnd, Container parent)
        {
            this.Parent = parent;
            this.Size = this.Parent.Size;

            CharacterString = Characters[rnd];
            CharacterTexture = Content.GetTexture(@"characters\" + Regex.Replace(CharacterString.ToLower(), @"\s", "_", RegexOptions.Multiline));

            var center = new Point(this.Size.X / 2, this.Size.Y / 2);
            int centerLeft = center.X / 2;
            int centerRight = center.X + (center.X / 2);

            var imgSize = ResizeKeepAspect(new Point(CharacterTexture.Width, CharacterTexture.Height), this.Size.X / 2 - LoadScreenPanel.RIGHT_PADDING, this.Size.Y - LoadScreenPanel.TOP_PADDING);
            var imgCenter = new Point(centerLeft - (imgSize.X / 2), center.Y - (imgSize.Y / 2));

            SilhouetteFX.Parameters["TextureWidth"].SetValue((float)CharacterTexture.Width);
            SilhouetteFX.Parameters["GlowColor"].SetValue(Color.White.ToVector4());
            GlowFX.Parameters["TextureWidth"].SetValue((float)CharacterTexture.Width);
            GlowFX.Parameters["GlowColor"].SetValue(Color.White.ToVector4());

            CharacterImage = new Image(CharacterTexture)
            {
                Parent = this.Parent,
                SpriteBatchParameters = new SpriteBatchParameters(),
                Size = imgSize,
                Location = imgCenter,
                Visible = this.Visible
            };
            CharacterImage.SpriteBatchParameters.Effect = SilhouetteFX;
            CharacterImage.SpriteBatchParameters.BlendState = BlendState.NonPremultiplied;
            Result = false;
        }
        private static Point ResizeKeepAspect(Point src, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.X);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Y);

            decimal rnd = Math.Min(maxWidth / (decimal)src.X, maxHeight / (decimal)src.Y);
            return new Point((int)Math.Round(src.X * rnd), (int)Math.Round(src.Y * rnd));
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var center = new Point(this.Size.X / 2, this.Size.Y / 2);
            int centerLeft = center.X / 2;
            int centerRight = center.X + (center.X / 2);

            var left = Utils.DrawUtil.HorizontalAlignment.Left;
            var top = Utils.DrawUtil.VerticalAlignment.Top;

            string title = this.Result ? "It's " + CharacterString + '!' : "Who's that Character?";
            string wrappedTitle = Utils.DrawUtil.WrapText(this.Font, title, this.Width / 2 - LoadScreenPanel.RIGHT_PADDING);
            int titleHeight = (int)this.Font.MeasureString(wrappedTitle).Height;
            int titleWidth = (int)this.Font.MeasureString(wrappedTitle).Width;
            var titleCenter = new Point(centerRight - (titleWidth / 2), center.Y - (titleHeight / 2));
            spriteBatch.DrawStringOnCtrl(this, wrappedTitle, this.Font, new Rectangle(titleCenter.X, titleCenter.Y, this.Size.X, this.Size.Y), Color.White, false, true, 2, left, top);
        }
    }
}
