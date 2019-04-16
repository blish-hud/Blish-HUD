using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Entities.Primitives;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Behavior;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Pathables;
using Blish_HUD.Pathing.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.WIC;

namespace Blish_HUD.Pathing.Markers {
    public class BasicMarkerPathable : ManagedPathable<Entities.Marker>, IMarker {
        
        private Vector2 _baseDimensions;
        private float _minimumSize = 1.0f;
        private float _maximumSize = 1.0f;
        private float _scale = 1.0f;
        private float _opacity = 1.0f;
        private string _text;
        private bool _active = false;

        public override float Scale {
            get => this.ManagedEntity.Scale;
            set {
                this.ManagedEntity.Scale = value; 
                OnPropertyChanged();
            }
        }

        public float MinimumSize {
            get => _minimumSize;
            set => SetProperty(ref _minimumSize, value);
        }

        public float MaximumSize {
            get => _maximumSize;
            set => SetProperty(ref _maximumSize, value);
        }

        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public override bool Active {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        public Texture2D Icon {
            get => this.ManagedEntity.Texture;
            set {
                this.ManagedEntity.Texture = value;
                OnPropertyChanged();
            }
        }

        public BasicMarkerPathable() : base(new Entities.Marker()) {
            //this.Behavior = new BasicTOBehavior<TacOMarkerPathable, Entities.Marker>(this, TacOBehavior.ReappearAfterTimer) {
            //    ZoneRadius = 4.5f,
            //    InfoText   = "You are in the zone!",
            //};
        }

    }
}
