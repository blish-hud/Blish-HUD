using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {

    [IdentifyingBehaviorAttributePrefix("copy")]
    public class Copy<TPathable, TEntity> : InZone<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public string CopyValue { get; set; }

        public int CopyRadius { get; set; } = 5;

        public string CopyMessage { get; set; } = "'{0}' copied to clipboard.";

        public Copy(TPathable managedPathable) : base(managedPathable) {
            this.ZoneRadius = 5;
        }

        public override void OnEnterZoneRadius(GameTime gameTime) {
            this.ManagedPathable.Active = false;

            System.Windows.Forms.Clipboard.SetText(this.CopyValue);

            Notification.ShowNotification(string.Format(this.CopyMessage, this.CopyValue));
        }

        public void LoadWithAttributes(IEnumerable<XmlAttribute> attributes) {
            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
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
