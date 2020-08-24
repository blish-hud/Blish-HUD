using System.Collections.Generic;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {
    [PathingBehavior("color")]
    public class Tint<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private Color _tintColor;

        public Color TintColor {
            get => _tintColor;
            set {
                _tintColor = value;
                UpdateTint();
            }
        }

        public Tint(TPathable managedPathable) : base(managedPathable) { /* NOOP */ }

        private void UpdateTint() {
            switch (this.ManagedPathable.ManagedEntity) {
                case Marker markerEntity:
                    markerEntity.TintColor = _tintColor;
                    break;
                case ScrollingTrail trailEntity:
                    trailEntity.TintColor = _tintColor;
                    break;
                default:
                    this.ManagedPathable.Behavior.Remove(this);
                    break;
            }
        }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes) {
            foreach (var attr in attributes) {
                switch (attr.Name) {
                    case "tint":
                    case "color":
                        if (!ColorUtil.TryParseHex(attr.Value, out _tintColor)) {
                            this.TintColor = Color.White;
                        }
                        UpdateTint();
                        break;
                }
            }
        }

    }
}
