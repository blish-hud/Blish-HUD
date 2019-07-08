using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Entities {
    public class EntityText : EntityBillboard {

        private CachedStringRender _cachedTextRender;

        private string _text      = string.Empty;
        private Color  _textColor = Color.White;

        public string Text {
            get => _text;
            set {
                if (SetProperty(ref _text, value)) {
                    UpdateTextRender();
                }
            }
        }

        public Color TextColor {
            get => _textColor;
            set {
                if (SetProperty(ref _textColor, value)) {
                    UpdateTextRender();
                }
            }
        }

        /// <inheritdoc />
        public EntityText(Entity attachedEntity) : base(attachedEntity) {
            this.AutoResizeBillboard = false;
        }

        private void UpdateTextRender() {
            var textSize = GameService.Content.DefaultFont32.MeasureString(_text);

            _cachedTextRender?.Dispose();

            if (!string.IsNullOrEmpty(_text)) {
                _cachedTextRender = CachedStringRender.GetCachedStringRender(_text,
                                                                             GameService.Content.DefaultFont32,
                                                                             new Rectangle(0, 0, (int) textSize.Width, (int) textSize.Height),
                                                                             _textColor,
                                                                             false,
                                                                             true);

                this.Size = _cachedTextRender.DestinationRectangle.Size.ToVector2().ToWorldCoord() / 2;

                this.Texture = _cachedTextRender.CachedRender;
            }
        }

    }
}
