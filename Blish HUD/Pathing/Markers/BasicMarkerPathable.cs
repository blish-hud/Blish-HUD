using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Markers {
    public class BasicMarkerPathable : ManagedPathable<Entities.Marker>, IMarker {
        
        private float _minimumSize = 1.0f;
        private float _maximumSize = 1.0f;
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

        public BasicMarkerPathable() : base(new Entities.Marker()) { }

    }
}
