using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Blish_HUD.Modules.EventTimers {
    public class MenuSection : Controls.Panel {

        private const int TITLE_HEIGHT = 32;

        private static Texture2D spriteSectionTitle;
        private static Texture2D spriteSectionTLBead;
        private static Texture2D spriteSectionBRBead;

        private string _title = "";
        public string Title {
            get => _title;
            set {
                if (_title == value) return;

                _title = value;

                OnPropertyChanged("Title");
            }
        }
        
        public MenuSection() {
            spriteSectionTitle = spriteSectionTitle ?? Content.GetTexture("156387");
            spriteSectionTLBead = spriteSectionTLBead ?? Content.GetTexture("1002144-fvh");
            spriteSectionBRBead = spriteSectionBRBead ?? Content.GetTexture("1002144-fv");

            //this.OnResized += delegate { UpdateRegion(); };
        }

        private void UpdateRegion() {
            this.ContentRegion = new Rectangle(
                                               9,
                                               2                     + TITLE_HEIGHT,
                                               this.Width - 15       - 6,
                                               this.Height - 12 - 12 - TITLE_HEIGHT
                                              );
        }

        public override void PaintContainer(SpriteBatch spriteBatch, Rectangle bounds) {
            base.PaintContainer(spriteBatch, bounds);

            spriteBatch.Draw(spriteSectionTitle, new Rectangle(9, 0, bounds.Width - 15 - 6, TITLE_HEIGHT), Color.White * 0.8f);

            //spriteBatch.Draw(spriteSectionTLBead, spriteSectionTLBead.Bounds.OffsetBy(0, spriteSectionTitle.Height - 10), Color.White);
            //spriteBatch.Draw(spriteSectionBRBead, spriteSectionBRBead.Bounds.OffsetBy(bounds.Right, bounds.Bottom).OffsetBy(-spriteSectionBRBead.Bounds.Size.X, -spriteSectionBRBead.Bounds.Size.Y), Color.White);


        }

    }
}
