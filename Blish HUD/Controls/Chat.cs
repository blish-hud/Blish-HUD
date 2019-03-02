using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    // TODO: The Chat control needs a lot more work before it's done
    class Chat:Control {

        private string _text;
        public string Text {
            get => _text;
            set {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(GameServices.GetService<ContentService>().GetTexture("chat-basic-header"), bounds, Color.White);
            spriteBatch.DrawString(Overlay.font_def12, this.Text, new Vector2(bounds.X + 181, bounds.Y + 121), Color.Black);
            spriteBatch.DrawString(Overlay.font_def12, this.Text, new Vector2(bounds.X + 180, bounds.Y + 120), Color.White);
            spriteBatch.Draw(GameServices.GetService<ContentService>().GetTexture("chat-actionsection"), new Rectangle(bounds.X + 120, bounds.Y + 150, 1024, 1024), Color.White);
        }

    }
}
