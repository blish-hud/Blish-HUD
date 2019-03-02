using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class BackButton : ScrollingButton {
        
        private string _text = "button";
        public string Text {
            get => _text;
            set {
                if (_text == value) return;

                _text = value;
                OnPropertyChanged();
            }
        }

        private string _navTitle;
        public string NavTitle {
            get => _navTitle;
            set {
                if (_navTitle == value) return;
                _navTitle = value;

                OnPropertyChanged();
            }
        }

        private Window _window;

        public BackButton(Window window) : base() {
            this.Size = new Point(280, 54);

            _window = window;
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            _window.NavigateBack();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(Content.GetTexture("784268"), new Rectangle(9, 9, 36, 36), Color.White);
            
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size16, ContentService.FontStyle.Regular), $"{this.Text}: {this.NavTitle}", new Rectangle(54, 0, this.Width - 54, this.Height), Color.White * 0.8f, DrawUtil.HorizontalAlignment.Left, DrawUtil.VerticalAlignment.Middle);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size16, ContentService.FontStyle.Regular), $"{this.Text}:", new Rectangle(54, 0, this.Width - 54, this.Height), Color.White * 0.8f, DrawUtil.HorizontalAlignment.Left, DrawUtil.VerticalAlignment.Middle);
        }

    }
}
