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
        public Texture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private string _name;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _description;
        public string Description {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private bool _active;
        public bool Active {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        private BHGw2Api.Landmark _landmark;
        public BHGw2Api.Landmark Landmark {
            get => _landmark;
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
                spriteBatch.DrawOnCtrl(this, Content.GetTexture("item-hover"), new Rectangle(Point.Zero, _size), Color.White * 0.5f);

            spriteBatch.DrawOnCtrl(this, _icon, new Rectangle((int)iconPadding, (int)iconPadding, ICON_SIZE, ICON_SIZE));
            spriteBatch.DrawStringOnCtrl(this, _name, Content.DefaultFont14, new Rectangle((int)(iconPadding * 2 + ICON_SIZE), 0, _size.X - (int)(iconPadding * 2) - ICON_SIZE, 20), Color.White, false, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Bottom);
            spriteBatch.DrawStringOnCtrl(this, _description, Content.DefaultFont14, new Rectangle((int)(iconPadding * 2 + ICON_SIZE), 20, _size.X - (int)(iconPadding * 2) - ICON_SIZE, 16), ContentService.Colors.Chardonnay, false, Utils.DrawUtil.HorizontalAlignment.Left, Utils.DrawUtil.VerticalAlignment.Top);
        }

    }
}
