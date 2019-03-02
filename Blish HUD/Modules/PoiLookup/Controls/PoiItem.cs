using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.PoiLookup {
    public class PoiItem:Controls.Control {

        private const int ICON_SIZE = 32;

        private Texture2D _icon;
        public Texture2D Icon { get { return _icon; } set { if (_icon != value) { _icon = value; Invalidate(); } } }

        private string _name;
        public string Name { get { return _name; } set { if (_name != value) { _name = value; Invalidate(); } } }

        private string _description;
        public string Description { get { return _description; } set { if (_description != value) { _description = value; Invalidate(); } } }

        private bool _active;
        public bool Active {
            get => _active;
            set {
                if (_active == value) return;

                _active = value;

                OnPropertyChanged();
            }
        }

        private BHGw2Api.Landmark _landmark;
        public BHGw2Api.Landmark Landmark {
            get { return _landmark; }
            set {
                if (_landmark != value && value != null) {
                    _landmark = value;
                    if (this.Landmark.Icon != null)
                        _icon = BHGw2Api.RenderService.GetTexture(this.Landmark.Icon);
                    else
                        _icon = this.Landmark.Type == "waypoint" ? Content.GetTexture("waypoint") : Content.GetTexture("poi");
                    _name = this.Landmark.Name;
                    _description = this.Landmark.ChatLink;

                    // Dirty
                    if (this.Active) {
                        this.Active = false;
                        this.Active = true;
                    }

                    Invalidate();
                } else if (value == null) {
                    this.Visible = false;
                }
            }
        }

        public PoiItem() : base() {
            this.Size = new Point(100, 36);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            base.OnMouseEntered(e);

            this.Active = true;
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            Invalidate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            double iconPadding = (this.Height - ICON_SIZE) / 2f;

            if (this.Active)
                spriteBatch.Draw(Content.GetTexture("item-hover"), bounds, Color.White * 0.5f);

            spriteBatch.Draw(this.Icon, new Rectangle((int)iconPadding, (int)iconPadding, ICON_SIZE, ICON_SIZE), Color.White);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular), this.Name, new Rectangle((int)(iconPadding * 2 + ICON_SIZE), 0, this.Width - (int)(iconPadding * 2) - ICON_SIZE, 20), Color.White, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Bottom);
            Utils.DrawUtil.DrawAlignedText(spriteBatch, Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular), this.Description, new Rectangle((int)(iconPadding * 2 + ICON_SIZE), 20, this.Width - (int)(iconPadding * 2) - ICON_SIZE, 16), ContentService.Colors.Chardonnay, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Top);
        }

    }
}
