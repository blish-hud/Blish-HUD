using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.BHUDControls.Pathing {

    public class Category : Controls.Control {

        public static Dictionary<string, Category> Categories = new Dictionary<string, Category>();

        protected List<Category> Subcategories = new List<Category>();

        public string CategoryPath { get; set; }
        public string DisplayName { get; set; }

        public Texture2D Icon { get; set; }

        public float HeightOffset { get; set; }

        public float FadeFar { get; set; }
        public float FadeNear { get; set; }

        public Point MinimumSize { get; set; } = new Point(1);

        public float Alpha { get; set; } = 1f;

        public bool CategoryEnabled { get; set; } = true;

        public Category(string categoryPath) {
            this.Size = new Point(64, 64);


        }

        //private Category GetCategoryOwner(IEnumerable<string> categoryPath, Category currentOwner) {
        //    if (categoryPath.Any()) {
        //        string first = categoryPath.First().ToLower();

        //        if (currentOwner == null && Category.Categories.ContainsKey(first))
        //            return GetCategoryOwner(categoryPath.Skip(1), Category.Categories[first]);
        //        else if (currentOwner.Subcategories.ContainsKey(first))
        //            return GetCategoryOwner(categoryPath.Skip(1), currentOwner.Subcategories[first]);
        //    } else if (currentOwner == null) {
        //        Category.Categories.Add(this.CategoryPath);
        //        return currentOwner;
        //    }
        //}

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            int sizex = Math.Min(this.Icon.Width, 64);
            int sizey = Math.Min(this.Icon.Height, 64);
            int posx = 32 - sizex / 2;
            int posy = 32 - sizey / 2;

            spriteBatch.Draw(this.Icon, new Rectangle(posx, posy, sizex, sizey), Color.White);
            spriteBatch.Draw(Content.GetTexture("605003"), bounds, Color.White);
        }

    }
}
