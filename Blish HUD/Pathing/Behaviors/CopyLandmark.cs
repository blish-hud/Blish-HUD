using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {

    [IdentifyingBehaviorAttributePrefix("landmark")]
    public class CopyLandmark<TPathable, TEntity> : InZone<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public string LandmarkCode { get; set; }

        public CopyLandmark(TPathable managedPathable) : base(managedPathable) {
            this.ZoneRadius = 5;
        }

        public override void OnEnterZoneRadius(GameTime gameTime) {
            this.ManagedPathable.Active = false;

            System.Windows.Forms.Clipboard.SetText(LandmarkCode);

            Controls.Notification.ShowNotification(GameService.Content.GetTexture("waypoint"), "Landmark copied to clipboard.", 2);
        }

        public void LoadWithAttributes(IEnumerable<XmlAttribute> attributes) {
            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
                    case "landmark":
                        this.LandmarkCode = attr.Value;
                        break;
                }
            }
        }

    }

}
