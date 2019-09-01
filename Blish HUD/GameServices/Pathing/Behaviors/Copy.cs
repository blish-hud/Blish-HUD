using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors.Activator;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {

    [PathingBehavior("copy")]
    public class Copy<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public string CopyValue { get; set; }

        public int CopyRadius { get; set; } = 5;

        public string CopyMessage { get; set; } = "'{0}' copied to clipboard.";

        public Copy(TPathable managedPathable) : base(managedPathable) {
            var zoneActivator = new ZoneActivator<TPathable, TEntity>(this) {
                ActivationDistance = 5f,
                DistanceFrom       = DistanceFrom.Player
            };
        }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes) {
            foreach (var attr in attributes) {
                switch (attr.Name.ToLower(CultureInfo.InvariantCulture)) {
                    case "copy":
                        this.CopyValue = attr.Value;
                        break;
                    case "copy-radius":
                        this.CopyRadius = int.Parse(attr.Value);
                        break;
                    case "copy-message":
                        this.CopyMessage = attr.Value;
                        break;
                }
            }
        }

    }

}
