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
    public class GamingTip : Control {
        public static List<string> Tips = new List<string>()
        {
            "You can cancel skill castings if you bind a key to 'Stow/Draw Weapons' in your Control Options.",
            "There are five known Tengu clans. They all fervently uphold three basic virtues: honour, family, and history.",
            "You can bind separate keys to each mount to ease the use and the switch.",
            "If you press the 'Dodge' and 'Jump' key at the same time, you'll make a 'Dodge Jump' which is a bit longer and useful in some occasion.",
            "Try not to knock away enemies if you have knockbacks, it's strong but sometimes it's better to fight them right then and there.",
            "Dodge rolling is a powerful tool, it allows you to avoid nearly anything thrown at you.",
            "Crowd Control will break the blue bar on certain enemies, use Hard CC to break it faster.",
            "Most skills have a target limit, make sure to check it because buffs are also limited by that number!",
            "Hard CC refers to skills that Daze, Float, Knockback, Knockdown, Launch, Pull, Sink, Stun, Fear or Taunt.",
            "Blind, Chill, Cripple, Immobilize, Slow and Weakness are all examples of Soft CC.",
            "Bonus experience is earned based on how long a creature has been alive in the world.",
            "You can complete the level 10 personal story for a Black Lion Chest Key once per week.",
            "To split an item stack, drag the stack to another slot while holding down the Alt key.",
            "Shift-Clicking an item or waypoint will copy it to chat, Ctrl-Clicking will link it immediately.",
            "/wiki <search-term> can be used to browse the wiki for the related article.",
            "You can usually grant swiftness and superspeed to NPC's to make them walk faster.",
            "Raptor with long jump is fastest over flat land, while Jackal is superior over uneven terrain.",
            "Skills that have a combo finisher effect can be used with combo fields for an added effect.",
            "The more Skritt in a group, the more intelligent they are. A large colony can rival the cunning of the Asura.",
            "Crafting is a great way to quickly level up an alt.",
            "Spirit Shards can be used to convert materials in the Mystic Forge.",
            "Instead of blood, Sylvari have golden sap running through their veins.",
            "Charr cubs are raised in a Fahrar where they develope bonds with other cubs and form their own warbands.",
            "The Six refers to the six Human Gods: Balthazar, Dwayna, Grenth, Kormir, Lyssa, and Melandru.",
            "Asurans believe that the Human Gods, Norn Spirits and even the Elder Dragons are all just components of the Eternal Alchemy.",
            "When threatened or pained, Quaggans go through a full body transformation and rational thought loses out to destructive tendencies.",
            "Quaggans avoid the use of pronouns or self-reference because they find it to be offensive and self-centered.",
            "Kodan young are said to be weak and easily susceptible to outside influence. This is why they are rarely ever seen by outsiders.",
            "Norn can live long lives; up to 120 years old, while maintaining their good health and vitality. However, few die of old age.",
            "Although Ascended equipment shares the same stats, Legendary equipment has the ability to change stat combinations at will.",
            "Because of the dynamic leveling and experience in Guild Wars 2, it's still worthwhile to visit and explore lower level zones.",
            "The Dwarves were originally humanoids with flesh and blood, before they were transformed by the Rite of the Great Dwarf to fight off the Great Destroyer and its spawn in 1079 AE.",
            "The Mouvelian calendar begins counting years from the moment the gods left Tyria. This event is known as the Exodus. Years before this date are labeled BE (Before the Exodus). Years after this date are AE (After the Exodus).",
            "Skills that apply buffs have ranges. Position yourself in a way that will maximize the amount of players affected.",
            "The three great orders of Tyria are the Durmand Priory, Order of Whispers and Vigil. Their pact alliance was founded in 1325 AE.",
            "Boons such as Might, Fury, Quickness and Alacrity can have a significant impact on your combat prowess.",
            "The Firstborn are the twelve oldest Sylvari, who first emerged from the Pale Tree in 1302 AE.",
            "The term \"Stack\" or \"Stacking\" is used by organized parties to signifiy their group to stick together as close as possible.",
            "Pipe Organs are musical instruments that can be found at various locations. Playing one correctly may result in a hidden chest nearby.",
            "Across Tyria, one can find, aside from non-interactable cats, a number of cats and kittens that will accept certain food items, ingredients, or other specific items; some under specific circumstances.",
            "A red bar under your skills indicates that your target is too far away for your skill to reach.",
            "Mounts' health, stamina and engage skill damage are all separate and independent from the player's gear.",
            "Mounts' have additional action abilities which are bindable in your keyboard options. These allow many maneuver and ways of traversing.",
            "Moving the mouse pointer over each entry in the legend of your map will make any revealed point or scout pointing to not yet revealed renown hearts flash on the map, making them much easier to find.",
            "The LFG search bar allows to concatenate filters using whitespace as a separator. A minus (-) preceding a word will remove any party or squad listing containing the given word.",
            "You can also write \"consume\" or other parts of an item's type into the search box of your inventory or vault.",
            "Each race has a cultural colour palette that reflects the character of the species. This means a red color for a human may not look the same as a red color for a norn or charr.",
            "Ghosts of Ascalon, written by Matt Forbeck and Jeff Grubb, is the first of three novels that bridge the time between the original game series and Guild Wars 2.",
            "Edge of Destiny, written by J. Robert King, is the second of three novels to bridge the time between the original game series and Guild Wars 2.",
            "Sea of Sorrows, written by Ree Soesbee, is the third and final novel to bridge the time between the original game series and Guild Wars 2."
        };
        private string SelectedTip;
        private BitmapFont Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
        private BitmapFont BigFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        public float ReadingTime { get; private set; }
        public GamingTip(int rnd)
        {
            SelectedTip = Tips[rnd];
            ReadingTime = SelectedTip.Length / 45;
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var center = new Point((this.Size.X / 2), this.Size.Y / 2);
            var left = Utils.DrawUtil.HorizontalAlignment.Center;
            var top = Utils.DrawUtil.VerticalAlignment.Top;
            string title = "Did You Know:";
            int titleHeight = (int)this.BigFont.MeasureString(title).Height;
            int titleWidth = (int)this.BigFont.MeasureString(title).Width;
            var titleCenter = new Point(center.X - (titleWidth / 2), center.Y - (titleHeight / 2));
            spriteBatch.DrawStringOnCtrl(this, title, this.BigFont, new Rectangle(titleCenter.X, LoadScreenPanel.TOP_PADDING, titleWidth, titleHeight), Color.White, false, true, 2, left, top);
            string wrappedTip = Utils.DrawUtil.WrapText(this.Font, this.SelectedTip, this.Width - LoadScreenPanel.RIGHT_PADDING);
            int tipHeight = (int)this.Font.MeasureString(wrappedTip).Height;
            int tipWidth = (int)this.Font.MeasureString(wrappedTip).Width;
            var tipCenter = new Point(center.X - (tipWidth / 2), center.Y - (tipHeight / 2));
            spriteBatch.DrawStringOnCtrl(this, wrappedTip, this.Font, new Rectangle(tipCenter.X, tipCenter.Y, tipWidth, tipHeight), Color.White, false, true, 2, left, top);
        }
    }
}
