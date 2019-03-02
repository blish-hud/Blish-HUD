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
using Microsoft.Xna.Framework;
using Praeclarum.Bind;
using Panel = Blish_HUD.Controls.Panel;

namespace Blish_HUD {

    public class PathingService : GameService {

        public List<Marker> Markers { get; set; } = new List<Marker>();

        public readonly PathingCategory Categories = new PathingCategory("root") { Visible = true };

        protected override void Initialize() {
            // Subscribe to map changes so that we can hide or show markers for the new map
            GameService.Player.OnMapIdChanged += PlayerOnOnMapIdChanged;
        }

        protected override void Load() {
            GameService.Director.BlishHudWindow.AddTab("Markers and Paths", "marker-pathing-icon", BuildPanel(GameService.Director.BlishHudWindow.ContentRegion), int.MaxValue - 5);

            //testMarker = new Marker(GameService.Content.GetTexture("42683"), GameService.Player.Position, new Vector2(1, 1)) {
            //    MapId = -1
            //};
            
            //this.RegisterMarker(testMarker);
        }

        // TODO: This panel actually belongs to the Markers & Paths module: move it there
        private Panel BuildPanel(Rectangle bounds) {

            const string PC_THISMAP = "This Map";

            var psPanel = new Panel() {
                Size      = bounds.Size,
                CanScroll = false,
            };

            var packsPanel = new FlowPanel() {
                FlowDirection  = FlowPanel.ControlFlowDirection.LeftToRight,
                ControlPadding = 8,
                Location       = new Point(psPanel.Width - 630, 50),
                Size           = new Point(630,                 psPanel.Height - 50 - Panel.BOTTOM_MARGIN),
                Parent         = psPanel,
                CanScroll      = true,
            };

            for (int i = 0; i < 25; i++) {

                var detailButtonTest1 = new DetailsButton() {
                    Parent   = packsPanel,
                    IconSize = DetailsIconSize.Large,
                    Icon     = Content.GetTexture("1228232"),
                    Text     = "Name of the event! " + i.ToString()
                };

                var gicon1 = new GlowButton() {
                    Icon     = Content.GetTexture("waypoint"),
                    Location = new Point(1, 1),
                    Parent   = detailButtonTest1
                };

                var detailButtonTest2 = new DetailsButton() {
                    Parent   = packsPanel,
                    IconSize = DetailsIconSize.Small,
                    Icon     = Content.GetTexture("1228232"),
                    Text     = "Name of the event! " + i.ToString() 
                };

                var gicon2 = new GlowButton() {
                    Icon     = Content.GetTexture("102530"),
                    Location = new Point(1, 1),
                    Parent   = detailButtonTest2,
                    GlowColor = Color.Red
                };

                var gicon3 = new GlowButton() {
                    Icon     = Content.GetTexture("pathing-icon"),
                    Location = new Point(gicon2.Right + 2, 1),
                    Parent   = detailButtonTest2,
                    GlowColor = Color.Blue
                };

                bool ico = true;

                gicon3.Click += delegate {
                    gicon3.Icon = Content.GetTexture(ico ? "578853" : "pathing-icon");

                    ico = !ico;
                };

            }

            var menuPanel = new Panel {
                ShowBorder = true,
                Size       = new Point(psPanel.Width - packsPanel.Width - 15, packsPanel.Height + Panel.BOTTOM_MARGIN),
                Location   = new Point(5,                                     50),
                CanScroll  = true,
                Parent     = psPanel,
                Title      = "Marker and Path Categories"
            };

            var mpCategories = new Menu {
                Size           = menuPanel.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent         = menuPanel,
            };

            // Create standard entries

            var thisMapMenuItem = new MenuItem {
                Text = PC_THISMAP,
                Icon = Content.GetTexture("1431767"), // Current events icon
                Parent = mpCategories
            };

            void HandleCategoryChange(object sender, NotifyCollectionChangedEventArgs e, Controls.Container parentMenuItem) {
                var parentCategory = sender as PathingCategory;

                switch (e.Action) {
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        goto case NotifyCollectionChangedAction.Remove;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (PathingCategory category in e.NewItems) {
                            AddCategoryMenuItem(parentMenuItem, category);
                        }
                        break;
                    case NotifyCollectionChangedAction.Add:
                        foreach (PathingCategory category in e.NewItems) {
                            AddCategoryMenuItem(parentMenuItem, category);
                        }
                        break;
                }
            }

            void AddCategoryMenuItem(Controls.Container parentMenuItem, PathingCategory newCategory) {

                var newCategoryMenuItem = new Modules.MarkersAndPaths.Controls.CategoryMenuItem() {
                    Text     = newCategory.Name,
                    CanCheck = true,
                    Icon     = string.IsNullOrWhiteSpace(newCategory.Name) ? null : Content.GetTexture(newCategory.IconFile),
                    Parent   = parentMenuItem
                };

                Binding.Create(() => newCategoryMenuItem.Checked == newCategory.Visible);

                newCategory.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs args) {
                    HandleCategoryChange(sender, args, newCategoryMenuItem);
                };

            }

            this.Categories.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs args) {
                HandleCategoryChange(sender, args, mpCategories);
            };

            return psPanel;
        }

        private void RemoveCategoryMenuItem(PathingCategory parentCategory, PathingCategory newCategory) {
            Console.WriteLine("A category was removed, but updating the MenuItems to reflect that hasn't been implemented yet!");
            // TODO: Implement RemoveCategoryMenuItem
        }

        private void ProcessMarkerState(Marker marker) {
            if (marker.MapId == Player.MapId || marker.MapId == -1) {
                Graphics.World.Entities.Add(marker);
            } else if (Graphics.World.Entities.Contains(marker)) {
                Graphics.World.Entities.Remove(marker);
            }
        }

        private void PlayerOnOnMapIdChanged(object sender, EventArgs e) {
            foreach (var marker in this.Markers) {
                ProcessMarkerState(marker);
            }
        }

        public void RegisterMarker(Marker newMarker) {
            this.Markers.Add(newMarker);
            ProcessMarkerState(newMarker);
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

        public string DisplayName { get; set; }

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

        private bool _visible;
        public bool Visible {
            //get => this.Parent?.Visible ?? true && _visible;
            get => _visible;
            set {
                if (_visible == value) return;

                _visible = value;
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

            // Not sure you even got here...
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
