using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Blish_HUD.PersistentStore;
using Microsoft.Xna.Framework;
using Panel = Blish_HUD.Controls.Panel;

namespace Blish_HUD {

    public class PathingService : GameService {

        public const string MARKER_DIRECTORY = "markers";

        private const string PATHING_STORENAME = "Pathing";

        public event EventHandler<EventArgs> NewMapLoaded;

        private readonly ConcurrentQueue<IPathable<Entity>> _queuedAddPathables = new ConcurrentQueue<IPathable<Entity>>();
        private readonly ConcurrentQueue<IPathable<Entity>> _queuedRemovePathables = new ConcurrentQueue<IPathable<Entity>>();

        public List<IPathable<Entity>> Pathables { get; set; } = new List<IPathable<Entity>>();
        
        public SynchronizedCollection<PathableResourceManager> PackManagers { get; set; } = new SynchronizedCollection<PathableResourceManager>();

        private Store _pathingStore;

        public Store PathingStore => _pathingStore ?? (_pathingStore = Store.RegisterStore(PATHING_STORENAME));

        protected override void Initialize() {
            // Subscribe to map changes so that we can hide or show markers for the new map
            Player.MapIdChanged += PlayerMapIdChanged;
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
                IconContextMenu.Show(Icon);
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

        private void PlayerMapIdChanged(object sender, EventArgs e) {
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

        public void UnregisterPathableResourceManager(PathableResourceManager pathableContext) {
            this.PackManagers.Remove(pathableContext);
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

        protected override void Unload() { /* NOOP */ }

    }

}
