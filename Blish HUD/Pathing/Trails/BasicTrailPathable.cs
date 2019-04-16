using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Trails {
    public class BasicTrailPathable : ManagedPathable<Entities.Trail> {

        private float _scale = 1.0f;
        private bool _active = false;

        public override float Scale {
            get => _scale;
            set {
                if (SetProperty(ref _scale, value)) UpdateBounds();
            }
        }

        public override bool Active {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        private void UpdateBounds() {
            Console.WriteLine($"{nameof(UpdateBounds)} was called despite it not being implemented yet!");
        }

        public BasicTrailPathable(Trail managedEntity) : base(managedEntity) { }
        
    }
}
