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
using Glide;
namespace Blish_HUD.Modules.LoadingScreenHints.Controls {
    public class LoadScreenPanel : Container {

        public const int TOP_PADDING = 20;
        public const int RIGHT_PADDING = 40;

        private Random rand;
        public Glide.Tween Fade;
        private Control CurrentLoadScreenTip;
        private HashSet<int> ShuffledHints;
        private HashSet<int> SeenGamingTips;
        private HashSet<int> SeenNarrations;
        private HashSet<int> SeenGuessCharacters;
        public LoadScreenPanel() {
            this.rand = new Random();
            this.ShuffledHints = new HashSet<int>();
            this.SeenGamingTips = new HashSet<int>();
            this.SeenNarrations = new HashSet<int>();
            this.SeenGuessCharacters = new HashSet<int>();

            this.Size = new Point(600, 200); // set static bounds.

            UpdateLocation(null, null);
            Graphics.SpriteScreen.Resized += UpdateLocation;
        }

        public void FadeOut() {
            if (Fade != null) return;

            float duration = 2.0f;
            if (this.CurrentLoadScreenTip != null) {
                if (this.CurrentLoadScreenTip is GuessCharacter) {
                    GuessCharacter selected = (GuessCharacter)this.CurrentLoadScreenTip;
                    selected.Result = true;
                    duration = duration + 3.0f;
                } else if (this.CurrentLoadScreenTip is Narration) {
                    Narration selected = (Narration)this.CurrentLoadScreenTip;
                    duration = duration + selected.ReadingTime;
                } else if (this.CurrentLoadScreenTip is GamingTip) {
                    GamingTip selected = (GamingTip)this.CurrentLoadScreenTip;
                    duration = duration + selected.ReadingTime;
                }
            }
            Fade = Animation.Tweener.Tween(this, new { Opacity = 0.0f }, duration);
            Fade.OnComplete(() => {
                this.Visible = false;
                this.NextHint();
                Fade.Cancel();
                Fade = null;
            });

        }
        public void NextHint() {
            int total = 3;
            int count = ShuffledHints.Count;
            if (count >= total) { ShuffledHints.Clear(); count = 0; }
            var range = Enumerable.Range(1, total).Where(i => !ShuffledHints.Contains(i));
            int index = rand.Next(0, total - count - 1);
            int hint = range.ElementAt(index);

            ShuffledHints.Add(hint);

            if (CurrentLoadScreenTip != null) {
                if (CurrentLoadScreenTip is GuessCharacter) {
                    GuessCharacter selected = (GuessCharacter)CurrentLoadScreenTip;
                    selected.CharacterImage.Dispose();
                }
                CurrentLoadScreenTip.Dispose();
            }

            switch (hint) {
                case 1:

                    total = GamingTip.Tips.Count;
                    count = SeenGamingTips.Count;
                    if (count >= total) { SeenGamingTips.Clear(); count = 0; }
                    range = Enumerable.Range(0, total - 1).Where(i => !SeenGamingTips.Contains(i));
                    index = rand.Next(0, total - count - 1);
                    hint = range.ElementAt(index);

                    SeenGamingTips.Add(hint);
                    CurrentLoadScreenTip = new GamingTip(hint) { Parent = this, Size = this.Size, Location = new Point(0, 0) };

                    break;

                case 2:

                    total = Narration.Narratives.Count;
                    count = SeenNarrations.Count;
                    if (count >= total) { SeenNarrations.Clear(); count = 0; }
                    range = Enumerable.Range(0, total - 1).Where(i => !SeenNarrations.Contains(i));
                    index = rand.Next(0, total - count - 1);
                    hint = range.ElementAt(index);

                    SeenNarrations.Add(hint);
                    CurrentLoadScreenTip = new Narration(hint) { Parent = this, Size = this.Size, Location = new Point(0, 0) };

                    break;

                case 3:

                    total = GuessCharacter.Characters.Count;
                    count = SeenGuessCharacters.Count;
                    if (count >= total) { SeenGuessCharacters.Clear(); count = 0; }
                    range = Enumerable.Range(0, total - 1).Where(i => !SeenGuessCharacters.Contains(i));
                    index = rand.Next(0, total - count - 1);
                    hint = range.ElementAt(index);

                    SeenGuessCharacters.Add(hint);
                    CurrentLoadScreenTip = new GuessCharacter(hint, this) { Location = new Point(0, 0) };

                    break;

                default:
                    throw new NotSupportedException();
            }
            this.AddChild(CurrentLoadScreenTip);
        }

        private void UpdateLocation(object sender, EventArgs e) {
            this.Location = new Point((Graphics.SpriteScreen.Width / 2 - this.Width / 2), (Graphics.SpriteScreen.Height / 2 - this.Height / 2) + 300);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, Content.GetTexture("background_loadscreenpanel"), bounds, Color.White);
        }

    }
}
