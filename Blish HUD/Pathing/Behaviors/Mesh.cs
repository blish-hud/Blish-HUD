using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Entities;
using Blish_HUD.Entities.Primitives;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Behaviors {

    [PathingBehavior("mesh")]
    public class Mesh<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        private Mesh _meshEntity;

        /// <inheritdoc />
        public Mesh(TPathable managedPathable) : base(managedPathable) { }

        /// <inheritdoc />
        public override void Load() {
            var loadedMesh = GameService.Content.ContentManager.Load<Model>($@"models\{_meshName}");

            _meshEntity = new Mesh(loadedMesh) {
                Position = this.ManagedPathable.Position
            };

            ManagedPathable.Opacity = 0f;
            GameService.Graphics.World.Entities.Add(_meshEntity);
        }

        private string _meshName = string.Empty;

        /// <inheritdoc />
        public void LoadWithAttributes(IEnumerable<XmlAttribute> attributes) {
            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
                    case "mesh":
                        _meshName = attr.Value.Trim();
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
