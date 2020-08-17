using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {

    [PathingBehavior("title")]
    class Title<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public string TitleText {
            get => ManagedPathable.ManagedEntity.BasicTitleText;
            set => ManagedPathable.ManagedEntity.BasicTitleText = value;
        }

        public Color TitleColor {
            get => ManagedPathable.ManagedEntity.BasicTitleTextColor;
            set => ManagedPathable.ManagedEntity.BasicTitleTextColor = value;
        }

        public Title(TPathable managedPathable) : base(managedPathable) { }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes) {
            bool colorSet = false;

            foreach (var attr in attributes) {
                switch (attr.Name.ToLowerInvariant()) {
                    case "title":
                        this.TitleText = attr.Value;
                        break;
                    case "title-color":
                        switch (attr.Value.ToLowerInvariant()) {
                            case "white":
                                this.TitleColor = Color.White;
                                break;
                            case "yellow":
                                this.TitleColor = Control.StandardColors.Yellow;
                                break;
                            case "red":
                                this.TitleColor = Control.StandardColors.Red;
                                break;
                            case "green":
                                this.TitleColor = Color.FromNonPremultiplied(85, 221, 85, 255);
                                break;
                            default:
                                if (ColorUtil.TryParseHex(attr.Value, out var cOut)) this.TitleColor = cOut;
                                break;
                        }
                        colorSet = true;
                        break;
                    default:
                        break;
                }
            }

            if (!colorSet) {
                this.TitleColor = Color.FromNonPremultiplied(85, 221, 85, 255);
            }
        }

    }

}
