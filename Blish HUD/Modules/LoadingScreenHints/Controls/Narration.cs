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
namespace Blish_HUD.Modules.LoadingScreenHints.Controls {
    public class Narration : Control {
        public static List<string> Narratives = new List<string>()
        {
            "Scriptures of Dwayna, 115 BE | And when the world rang with the clanging of swords and did fire fall from the skies, Dwayna, goddess of life and air, heard the wailings and pleas of the weak.",
            "Scriptures of Melandru, 48 BE | And as She saith, so was it done. From their limbs sprouted branches, and the blood in their veins was the sap of trees.",
            "Orrian History Scrolls, The Six, Volume 3 — Melandru: Goddess of Nature | When she saw destruction, she brought creation. Where she saw anger, she grew love.",
            "Sacred text of Lyssa. Goddess of love | May no weapon sever the bond that holds your hands together, And may no word sever the love that keeps your hearts as one.",
            "Scriptures of Lyssa, 45 BE | True beauty is measured not by appearance but by actions and deeds.",
            "Scriptures of Grenth, 48 BE | The ground grew white with frost and ice, and from forth the frozen earth spilled the rotted, skeletal minions of Grenth.",
            "Scriptures of Kormir, 1075 AE | And so it came to pass that Spearmarshal Kormir, hero of all Elona, was pulled into the inky blackness surrounding the God of Secrets.",
            "Vigil Recruit | Wow. That's quality armor.",
            "Sylvari Banker | More violets, less violence.",
            "A Choya's Friends | Choya don't have many friends. The fun stops where the needle ends.",
            "A Flight from Flame | Fighting begets fights, violence begets violence.",
            "A Kralkatorrid Affair | The dragon may have crystallized my body, but you, Hubert...you crystallized my heart.",
            "A Posy of Forget-Me-Nots | I am midnight, and all night creatures know me.",
            "Bandit Journal | That damned Gorseval thing keeps killing everything that goes near it.",
            "Battered Book, Invictus Castrum | Dredge are so ugly. It took us weeks to notice some were female.",
            "Biography of Gwen Thackeray | Shaped by a life of loss and victory, driven by ferocity and cunning, Gwen Thackeray is indisputably one of Ascalon's most influential post-Searing figures.",
            "Biography of Ogden Stonehealer | The Great Dwarf and the Great Destroyer met head-on, and the fate of all dwarves was forged in the fires of war.",
            "Bjornjan the Tall | Bjornjan once ripped the horns off a minotaur, then used them to climb onto the back of a great snow wurm and ride it through the Jaga Moraine.",
            "The Snow Hunt | Welcome to Osbert Bleake's Nightmares for Naughty Children, a series of children's books designed to scare the snot, wit and spittle out of them.",
            "Nameless Book, Godslost Swamp | The air itself feels as though it's made of teeth. The voice grows stronger as two words emerge in a horrid clarify: 'I'm free.'",
            "Owl, Spirit of the Wild | Those who charge blindly forward are likely to be surprised.",
            "Owl, Spirit of the Wild | Brother, all your hunting is for naught if your family dines alone.",
            "Damaged Writing, Owl's Abattoir | ...there came a cry that carried not through the air, but through the heart of every norn. It was then that we knew Owl was lost to us.",
            "Discourse on a Tome, 1075 AE | Prophecies are inherently misleading.",
            "Discarded Legendary Relic Concepts | There were also at least two cases where the Cephalocus tried to eat its wielder, which focus groups described as 'not ideal' and 'absolutely terrifying.'",
            "Dwarven Defensive Strategies | Taller opponents fall harder. Strike low to knock them off balance, then finish them off with a kick to the knee or a head butt.",
            "Everyday Healing: Gift of the Monk | Remember, the road is long, but it need not be taken alone.",
            "History of Tyria | Races once locked in bitter war now live on the same streets, work in the same neighborhoods, and trade in the same marketplaces. And today, Tyria unites in the struggle against the dragons.",
            "How to Grow a Choya | Don't.",
            "Icehammer's Journal | If other races do not heed our warnings and unite, Jormag will rain destruction upon their unprepared armies. And, it will be our fault for leading him here.",
            "In Consideration of Sacrifice | To know when to sacrifice one for the many is never easy.",
            "Bandit Journal  |  We should either put them to work or watch them get eaten by Gorseval. I could use some entertainment.",
            "Journal of Kormir | The destruction of Tyria is inevitable, yet there is a part of me that still remembers what it was like to be mortal—what it is to hope.",
            "Koss on Koss, Volume Two | What if there's no longer a place for an old warrior like me?",
            "Master Togo's Book of Koans 1 | I have learned, Master, that when your opponent is stronger than you, it is good to wait until he blows himself out.",
            "Nightmare Courtiers | They seek to inject Nightmare into the Dream by torturing their fellow sylvari and breaking their minds.",
            "Of Maelstroms | Nature has presented us with new challenges, and if we must adapt to survive these trials, we will.",
            "Personal Log, Rata Primus | How can I run a proper lab if he's going to bawl his eyes out every time we liquidate a few dozen specimens?",
            "Private Property of P.I. Joko | Allow me to catch you up: Everyone in Kryta is an idiot and I hate it here.",
            "Quotes from Ventari the Centaur | The outside world is a violent place, where the seeds of chaos are sown with innocent tears.",
            "Quotes from Ventari the Centaur | All things have a right to grow. The blossom is brother to the weed.",
            "Quotes from Ventari the Centaur | Never leave a wrong to ripen into evil or sorrow. Act with wisdom, but act. From the smallest blade of grass to the largest mountain, where life goes—so, too, should you.",
            "Quotes from Ventari the Centaur | Live life well and fully, and waste nothing. Do not fear difficulty. Hard ground makes stronger roots.",
            "Recipe for Rancidity | Ignore any strange noises of pleas for release coming from the kettle.",
            "Scriptures of Kormir, 1075 AE | And though her sight had been robbed, her body wracked, and her spirit flayed, she remained resolute.",
            "The Aidan Journal | The wilderness calls to you. Answer it and run with me.",
            "The Awakening | We must not falter. Kryta is at stake, and no one else can save her.",
            "The Elder Dragons | They are ancient and unfathomable. They embody concepts of nature - fire, ice, crystal, even death - but their very presence threatens to destroy the world.",
            "Mad King Thorn | You bottomless bipedal beluga, I'd strangle you if somebody hadn't already beat me to it!",
            "The Map of the All | Should the energies become imbalanced, the world will tilt and all beings will fall off it into the void."
        };
        private string SelectedNarration;
        private string SelectedSource;
        private BitmapFont Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
        public float ReadingTime { get; private set; }
        public Narration (int rnd)
        {
            var split = Narratives[rnd].Split('|');
            SelectedNarration = '“' + split[1].Trim() + '”';
            SelectedSource = split[0].Trim();
            ReadingTime = SelectedNarration.Length / 45;
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            var center = new Point(this.Size.X / 2, this.Size.Y / 2);
            int centerRight = center.X + (center.X / 2);

            var left = Utils.DrawUtil.HorizontalAlignment.Center;
            var top = Utils.DrawUtil.VerticalAlignment.Top;

            string citation = Utils.DrawUtil.WrapText(this.Font, SelectedNarration, this.Width - LoadScreenPanel.RIGHT_PADDING);
            string sourceBind = "— ";
            int srcBindWidth = (int)this.Font.MeasureString(sourceBind).Width;
            int srcBindHeight = (int)this.Font.MeasureString(sourceBind).Height;
            string source = Utils.DrawUtil.WrapText(this.Font, SelectedSource, centerRight / 2);

            int srcHeight = (int)this.Font.MeasureString(source).Height;
            int srcWidth = (int)this.Font.MeasureString(source).Width;
            var srcCenter = new Point(center.X - (srcWidth / 2), center.Y - (srcHeight / 2));

            int textHeight = (int)this.Font.MeasureString(citation).Height + srcHeight;
            int textWidth = (int)this.Font.MeasureString(citation).Width;
            var textCenter = new Point(center.X - (textWidth / 2), center.Y - (textHeight / 2));
            spriteBatch.DrawStringOnCtrl(this, citation, this.Font, new Rectangle(textCenter.X, textCenter.Y, textWidth, textHeight), Color.White, false, true, 2, left, top);

            int srcPaddingY = textCenter.Y + (textHeight / 2) + (this.Font.LineHeight);
            int srcBindPaddingX = centerRight - (srcWidth / 2) - srcBindWidth;
            spriteBatch.DrawStringOnCtrl(this, sourceBind, this.Font, new Rectangle(srcBindPaddingX, srcPaddingY, srcBindWidth, srcBindHeight), Color.White, false, true, 2, left, top);

            int srcPaddingX = centerRight - (srcWidth / 2);
            spriteBatch.DrawStringOnCtrl(this, source, this.Font, new Rectangle(srcPaddingX, srcPaddingY, srcWidth, srcHeight), Color.White, false, true, 2, left, top);
        }
    }
}
