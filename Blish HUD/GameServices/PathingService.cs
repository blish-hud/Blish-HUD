using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Blish_HUD.Annotations;
using Blish_HUD.Controls;
using Blish_HUD.Custom.Collections;
using Blish_HUD.Entities;
using Blish_HUD.Entities.Paths;
using Microsoft.Xna.Framework;
using Praeclarum.Bind;
using Panel = Blish_HUD.Controls.Panel;

namespace Blish_HUD {

    public class PathingService : GameService {

        public List<Marker> Markers { get; set; } = new List<Marker>();
        public List<Path> Paths { get; set; } = new List<Path>();

        public readonly PathingCategory Categories = new PathingCategory("root") { Visible = true };

        protected override void Initialize() {
            // Subscribe to map changes so that we can hide or show markers for the new map
            GameService.Player.OnMapIdChanged += PlayerOnOnMapIdChanged;
        }

        protected override void Load() {
            //GameService.Director.BlishHudWindow.AddTab("Markers and Paths", "marker-pathing-icon", BuildPanel(GameService.Director.BlishHudWindow.ContentRegion), int.MaxValue - 5);

            // We will actually just be using a CornerIcon for now
            BuildPanel(Rectangle.Empty);
        }

        private Panel BuildPanel(Rectangle bounds) {
            const string PC_THISMAP = "This Map";

            var psIcon = new CornerIcon() {
                BasicTooltipText = "Markers and Paths",
                Icon             = Content.GetTexture("marker-pathing-icon")
            };

            var psContextMenu = new ContextMenuStrip();
            var thisMap = psContextMenu.AddMenuItem(PC_THISMAP);

            var allMarkers = psContextMenu.AddMenuItem("All Markers");

            var rootCategoryMenu = new ContextMenuStrip();

            allMarkers.Submenu = rootCategoryMenu;

            psIcon.Click += delegate {
                psContextMenu.Show(psIcon.Location + psIcon.Size);
            };

            // Handle adding loaded categories

            void HandleCategoryChange(object sender, NotifyCollectionChangedEventArgs e, ContextMenuStrip parentMenuItem) {
                var parentCategory = sender as PathingCategory;

                switch (e.Action) {
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        goto case NotifyCollectionChangedAction.Remove;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (PathingCategory category in e.OldItems) {
                            RemoveCategoryMenuItem(parentCategory, category);
                        }
                        break;
                    case NotifyCollectionChangedAction.Add:
                        foreach (PathingCategory category in e.NewItems) {
                            AddCategoryMenuItem(parentMenuItem, category);
                        }
                        break;
                }
            }

            void AddCategoryMenuItem(ContextMenuStrip parentMenuItem, PathingCategory newCategory) {

                var newCategoryContextMenuItem = parentMenuItem.AddMenuItem(newCategory.Name);
                newCategoryContextMenuItem.CanCheck = true;

                //var newCategoryMenuItem = new Modules.MarkersAndPaths.Controls.CategoryMenuItem() {
                //    Text     = newCategory.Name,
                //    CanCheck = true,
                //    Icon     = string.IsNullOrWhiteSpace(newCategory.Name) ? null : Content.GetTexture(newCategory.IconFile),
                //    Parent   = parentMenuItem
                //};

                Binding.Create(() => newCategoryContextMenuItem.Checked == newCategory.Visible &&
                                     newCategoryContextMenuItem.Text == newCategory.DisplayName);

                

                newCategory.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs args) {
                    if (newCategoryContextMenuItem.Submenu == null)
                        newCategoryContextMenuItem.Submenu = new ContextMenuStrip();

                    HandleCategoryChange(sender, args, newCategoryContextMenuItem.Submenu);
                };

            }

            this.Categories.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs args) {
                HandleCategoryChange(sender, args, rootCategoryMenu);
            };

            return null;
        }

        private void RemoveCategoryMenuItem(PathingCategory parentCategory, PathingCategory oldCategory) {
            Console.WriteLine("A category was removed, but updating the MenuItems to reflect that hasn't been implemented yet!");
            // TODO: Implement RemoveCategoryMenuItem
            throw new NotImplementedException($"Function {nameof(RemoveCategoryMenuItem)} of {nameof(PathingService)} was called before it has been implemented in code.");
        }

        private void ProcessMarkerState(Marker marker) {
            if (marker.MapId == Player.MapId || marker.MapId == -1) {
                Graphics.World.Entities.Add(marker);
            } else if (Graphics.World.Entities.Contains(marker)) {
                Graphics.World.Entities.Remove(marker);
            }
        }

        private void ProcessPathState(Path path) {
            if (path.MapId == Player.MapId || path.MapId == -1) {
                Graphics.World.Entities.Add(path);
            } else if (Graphics.World.Entities.Contains(path)) {
                Graphics.World.Entities.Remove(path);
            }
        }

        private void PlayerOnOnMapIdChanged(object sender, EventArgs e) {
            foreach (var marker in this.Markers) {
                ProcessMarkerState(marker);
            }

            foreach (var path in this.Paths) {
                ProcessPathState(path);
            }
        }

        public void RegisterMarker(Marker newMarker) {
            this.Markers.Add(newMarker);
            ProcessMarkerState(newMarker);
        }

        public void RegisterPath(Path newPath) {
            this.Paths.Add(newPath);
            ProcessPathState(newPath);
        }

        protected override void Update(GameTime gameTime) {

        }

        protected override void Unload() {

        }

    }
    
    [Serializable]
    public class PathingCategory :ObservableKeyedCollection<string, PathingCategory>, INotifyPropertyChanged {

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

        public string Namespace => (this.Parent != null && this.Parent != GameService.Pathing.Categories)
                                       ? $"{this.Parent.Namespace}.{this.Name}"
                                       : this.Name;

        private List<Entities.Marker> _markers = new List<Marker>();
        private List<Entities.Paths.Path> _paths = new List<Path>();

        private string _displayName;
        public string DisplayName {
            get => !string.IsNullOrWhiteSpace(_displayName) ? _displayName : this.Name;
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

                _markers.ForEach(m => m.Visible = _visible);
                _paths.ForEach(p => p.Visible = _visible);

                OnPropertyChanged();
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

        public float       Size         = 1.0f;
        public float       Alpha        = 1.0f;
        public float       FadeNear     = -1.0f;
        public float       FadeFar      = -1.0f;
        public float       Height       = 1.5f;

        // TODO: Implement POIBehavior
        //public POIBehavior Behavior     = POIBehavior.AlwaysVisible;

        public int         ResetLength  = 0;
        public int         ResetOffset  = 0;
        public int         AutoTrigger  = 0;
        public int         HasCountdown = 0;
        public float       TriggerRange = 2.0f;
        public int         MinSize      = 5;
        public int         MaxSize      = 2048;
        public Color       Color        = Color.White;
        public string      TrailData;
        public float       AnimSpeed = 1;
        public float       TrailScale = 1;
        public string      ToggleCategory;

        public PathingCategory(string name) {
            this.Name = name.ToLower();
        }

        public void AddMarker(Entities.Marker newMarker) {
            _markers.Add(newMarker);
        }

        public void AddPath(Entities.Paths.Path newPath) {
            _paths.Add(newPath);
        }

        public PathingCategory GetOrAddCategoryFromNamespace(string @namespace) {
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
