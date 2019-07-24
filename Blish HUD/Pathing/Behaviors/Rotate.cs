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

    [PathingBehavior("rotate")]
    class Rotate<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        public float RotationX {
            get => MathHelper.ToDegrees(ManagedPathable.ManagedEntity.RotationX);
            set => ManagedPathable.ManagedEntity.RotationX = MathHelper.ToRadians(value);
        }

        public float RotationY {
            get => MathHelper.ToDegrees(ManagedPathable.ManagedEntity.RotationY);
            set => ManagedPathable.ManagedEntity.RotationY = MathHelper.ToRadians(value);
        }

        public float RotationZ {
            get => MathHelper.ToDegrees(ManagedPathable.ManagedEntity.RotationZ);
            set => ManagedPathable.ManagedEntity.RotationZ = MathHelper.ToRadians(value);
        }

        public Rotate(TPathable managedPathable) : base(managedPathable) { }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes) {
            float rotateX = 0f;
            float rotateY = 0f;
            float rotateZ = 0f;

            foreach (var attr in attributes) {
                switch (attr.Name.ToLower()) {
                    case "rotate-x":
                        InvariantUtil.TryParseFloat(attr.Value, out rotateX);
                        break;
                    case "rotate-y":
                        InvariantUtil.TryParseFloat(attr.Value, out rotateY);
                        break;
                    case "rotate-z":
                        InvariantUtil.TryParseFloat(attr.Value, out rotateZ);
                        break;
                }
            }

            ManagedPathable.ManagedEntity.Rotation = new Vector3(MathHelper.ToRadians(rotateX),
                                                                 MathHelper.ToRadians(rotateY),
                                                                 MathHelper.ToRadians(rotateZ));
        }

    }

}
