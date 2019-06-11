using System;
using System.Collections.Concurrent;
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
using Blish_HUD.Modules.MarkersAndPaths;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Humanizer;
using Microsoft.Xna.Framework;
using Panel = Blish_HUD.Controls.Panel;

namespace Blish_HUD {

    public class PathingService : GameService {

        public const string MARKER_DIRECTORY = "markers";

        private const string PATHING_STORENAME = "Pathing";

        public event EventHandler<EventArgs> NewMapLoaded;

        private ConcurrentQueue<IPathable<Entity>> _queuedAddPathables = new ConcurrentQueue<IPathable<Entity>>();
        private ConcurrentQueue<IPathable<Entity>> _queuedRemovePathables = new ConcurrentQueue<IPathable<Entity>>();

        public List<IPathable<Entity>> Pathables { get; set; } = new List<IPathable<Entity>>();
        
        

        public List<PathableResourceManager> PackManagers { get; set; } = new List<PathableResourceManager>();

        private PersistentStore _pathingStore;

        public PersistentStore PathingStore => _pathingStore ?? (_pathingStore = GameService.Store.Stores.GetSubstore(PATHING_STORENAME));

        protected override void Initialize() {
            // Subscribe to map changes so that we can hide or show markers for the new map
            Player.OnMapIdChanged += PlayerOnOnMapIdChanged;
        }

        protected override void Load() {
           BuildCornerIcon();
        }

        public CornerIcon Icon;
        public ContextMenuStrip IconContextMenu;

        private Panel BuildCornerIcon() {
            Icon = new CornerIcon() {
                BasicTooltipText = "Pathing",
                Icon             = Content.GetTexture("marker-pathing-icon"),
                Parent           = Graphics.SpriteScreen,
                Priority         = Int32.MaxValue - 1,
            };

            IconContextMenu = new ContextMenuStrip();

            Icon.Click += delegate {
                IconContextMenu.Show(Icon.Location + Icon.Size);
            };

            return null;
        }

        private void ProcessPathableState(IPathable<Entity> pathable) {
            if (pathable.MapId == Player.MapId || pathable.MapId == -1) {
                //pathable.Active = true;
                Graphics.World.Entities.Add(pathable.ManagedEntity);
            } else if (Graphics.World.Entities.Contains(pathable.ManagedEntity)) {
                //pathable.Active = false;
                Graphics.World.Entities.Remove(pathable.ManagedEntity);
            }
        }

        private void ProcessAddedPathable(IPathable<Entity> pathable) {
            Graphics.World.Entities.Add(pathable.ManagedEntity);
            this.Pathables.Add(pathable);
        }

        private void ProcessRemovedPathable(IPathable<Entity> pathable) {
            Graphics.World.Entities.Remove(pathable.ManagedEntity);
            this.Pathables.Remove(pathable);
        }

        private void PlayerOnOnMapIdChanged(object sender, EventArgs e) {
            //for (int i = 0; i < this.Pathables.Count - 1; i++)
            //    ProcessPathableState(this.Pathables[i]);

            NewMapLoaded?.Invoke(this, EventArgs.Empty);

            foreach (var packContext in this.PackManagers)
                packContext.RunTextureDisposal();
        }

        public void RegisterPathable(IPathable<Entity> pathable) {
            if (pathable == null) return;

            _queuedAddPathables.Enqueue(pathable);
        }

        public void UnregisterPathable(IPathable<Entity> pathable) {
            if (pathable == null) return;

            _queuedRemovePathables.Enqueue(pathable);
        }

        public void RegisterPathableResourceManager(PathableResourceManager pathableContext) {
            if (pathableContext == null) return;

            this.PackManagers.Add(pathableContext);
        }

        protected override void Update(GameTime gameTime) {
            while (_queuedAddPathables.TryDequeue(out IPathable<Entity> queuedPathable)) {
                ProcessAddedPathable(queuedPathable);
            }

            while (_queuedRemovePathables.TryDequeue(out IPathable<Entity> queuedRemovedPathable)) {
                ProcessRemovedPathable(queuedRemovedPathable);
            }

            foreach (IPathable<Entity> pathable in this.Pathables) {
                if (!pathable.Active) continue;

                pathable.Update(gameTime);
            }
        }

        protected override void Unload() {

        }

    }

}
