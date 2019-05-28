using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Annotations;
using Blish_HUD.Custom.Collections;
using Blish_HUD.Entities;
using Blish_HUD.Pathing;
using Humanizer;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO {
    [Serializable]
    public class PathingCategory : ObservableKeyedCollection<string, PathingCategory>, INotifyPropertyChanged {

        private PathingCategory _parent;
        public PathingCategory Parent {
            get => _parent;
            set {
                if (_parent == value) return;

                // Remove us from parent, if we have one
                _parent?.Remove(this);

                // Assign parent to new parent and add us to new parent, if it exists
                _parent = value;
                _parent?.Add(this);

                //if (_parent != null) {
                //    _parent.PropertyChanged += delegate (object sender, PropertyChangedEventArgs args) {
                //        //var parentCategory = sender as PathingCategory;

                //        if (args.PropertyName == nameof(this.Visible))
                //            OnPropertyChanged(nameof(this.Visible));
                //    };
                //}

                OnPropertyChanged();
            }
        }

        public string Name { get; }

        public string Namespace => (this.Parent != null && this.Parent.Parent != null)
                                       ? $"{this.Parent.Namespace}.{this.Name}"
                                       : this.Name;

        //private List<Entities.Marker> _markers = new List<Marker>();
        //private List<Entities.Pathing.Paths.ITrail> _paths = new List<ITrail>();

        private List<IPathable> _pathables = new List<IPathable>();

        //public ReadOnlyCollection<Entities.Marker> Markers => _markers.AsReadOnly();
        //public ReadOnlyCollection<Entities.Pathing.Paths.ITrail> Paths => _paths.AsReadOnly();

        public ReadOnlyCollection<IPathable> Pathables => _pathables.AsReadOnly();

        private string _displayName;
        public string DisplayName {
            get => !string.IsNullOrEmpty(_displayName) ? _displayName : this.Name.Titleize();
            set {
                if (_displayName == value) return;

                _displayName = value;
                OnPropertyChanged();
            }
        }

        private string _iconFile;
        public string IconFile {
            get => !string.IsNullOrEmpty(_iconFile)
                       // Use the icon file specified by this category
                       ? _iconFile
                       // Inherit the icon file
                       : this.Parent?.IconFile;
            set {
                if (_iconFile == value) return;

                _iconFile = value;
                OnPropertyChanged();
            }
        }

        private bool _visible = true;
        public bool Visible {
            //get => this.Parent?.Visible ?? true && _visible;
            get => _visible;
            set {
                if (_visible == value) return;

                _visible = value;

                //UpdateMarkerState();
                UpdatePathableState();

                OnPropertyChanged();
            }
        }

        private bool _parentVisible = true;
        private bool ParentVisible {
            get => _parentVisible;
            set {
                if (_parentVisible == value) return;

                _parentVisible = value;

                //UpdateMarkerState();
                UpdatePathableState();
            }
        }

        private bool _enabled = true;
        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) return;

                _enabled = value;
                OnPropertyChanged();
            }
        }

        //public float       Size         = 1.0f;
        //public float       Alpha        = 1.0f;
        //public float       FadeNear     = -1.0f;
        //public float       FadeFar      = -1.0f;
        //public float       Height       = 1.5f;

        //// TODO: Implement POIBehavior
        ////public POIBehavior Behavior     = POIBehavior.AlwaysVisible;

        //public int         ResetLength  = 0;
        //public int         ResetOffset  = 0;
        //public int         AutoTrigger  = 0;
        //public int         HasCountdown = 0;
        //public float       TriggerRange = 2.0f;
        //public int         MinSize      = 5;
        //public int         MaxSize      = 2048;
        //public Color       Color        = Color.White;
        //public string      TrailData;
        //public float       AnimSpeed = 1;
        //public float       TrailScale = 1;
        //public string      ToggleCategory;

        public XmlNode SourceXmlNode { get; set; }

        public PathingCategory(string name) {
            this.Name = name.ToLower();
        }

        //public void UpdateMarkerState() {
        //    _markers.ForEach(m => m.Visible = this.ParentVisible && this.Visible);
        //    _paths.ForEach(p => p.Visible   = this.ParentVisible && this.Visible);

        //    foreach (var child in this) {
        //        child.ParentVisible = _parentVisible && this.Visible;
        //    }
        //}

        public void UpdatePathableState() {
            _pathables.ForEach(p => ((IPathable<Entity>)p).ManagedEntity.Visible = this.ParentVisible && this.Visible);

            foreach (var child in this) {
                child.ParentVisible = _parentVisible && this.Visible;
            }
        }

        //public void AddMarker(Entities.Marker newMarker) {
        //    _markers.Add(newMarker);
        //}

        //public void AddPath(Entities.Pathing.Paths.ITrail newTrail) {
        //    _paths.Add(newTrail);
        //}

        public void AddPathable(Pathing.IPathable pathable) {
            _pathables.Add(pathable);
        }

        public PathingCategory GetOrAddCategoryFromNamespace(string @namespace) {
            if (@namespace == null) return null;

            return this.GetOrAddCategoryFromNamespace(@namespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public PathingCategory GetOrAddCategoryFromNamespace(IEnumerable<string> splitNamespace) {
            List<string> namespaceSegments = splitNamespace.Select(ns => ns.ToLower()).ToList();

            string segmentValue = namespaceSegments[0];

            // Remove this namespace segment so that we can process this recursively
            namespaceSegments.RemoveAt(0);

            PathingCategory targetCategory;

            if (!this.Contains(segmentValue)) {
                // Subcategory was not already defined
                targetCategory = new PathingCategory(segmentValue) { Parent = this };
            } else {
                // Subcategory was already defined
                targetCategory = this[segmentValue];
            }

            return namespaceSegments.Any()
                       // Not at end of namespace - continue drilling
                       ? targetCategory.GetOrAddCategoryFromNamespace(namespaceSegments)
                       // At end of namespace - return target category
                       : targetCategory;
        }

        protected override string GetKeyForItem(PathingCategory item) {
            return item.Name.ToLower();
        }

        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e) {
            var childCat = sender as PathingCategory;

            // Not sure how you even got here...
            if (childCat == null) return;

            // Just something to let us waterfall back up
            OnPropertyChanged(nameof(this.Items));
        }

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
