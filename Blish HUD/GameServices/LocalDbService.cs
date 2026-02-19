using Blish_HUD.LocalDb;
using Microsoft.Xna.Framework;
using System.IO;

#nullable enable

namespace Blish_HUD {
    public class LocalDbService : GameService {

        private const string DIRECTORY_NAME = "localdb/";
        private const string META_FILENAME = "meta.json";
        private const string DATABASE_FILENAME = "localdb.sqlite";
        private const string LOCK_FILENAME = "localdb.lock";

        /// <summary>
        /// Get read-only access to metadata on all database collections.
        /// </summary>
        public IDbMeta Meta => _handler;

        private string _basePath = null!;
        private DbHandler _handler = null!;

        /// <summary>
        /// Get read-only access to all database collections. Make sure to call <c>DisposeAsync</c> or <c>Dispose</c>
        /// when you're done.
        /// </summary>
        /// <returns>An object used to access database collections.</returns>
        public IDbAccess GetAccess() {
            return _handler.GetAccess();
        }

        protected override void Initialize() {
            _basePath = DirectoryUtil.RegisterDirectory(Path.Combine(DirectoryUtil.CachePath, DIRECTORY_NAME));
        }

        protected override void Load() {
            _handler = new DbHandler(
                Path.Combine(_basePath, META_FILENAME),
                Path.Combine(_basePath, DATABASE_FILENAME),
                Path.Combine(_basePath, LOCK_FILENAME));

            UpdateCollections();

            Gw2Mumble.Info.BuildIdChanged += BuildIdChanged;
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

        protected override void Unload() {
            Gw2Mumble.Info.BuildIdChanged -= BuildIdChanged;

            _handler.Dispose();
        }

        private void BuildIdChanged(object sender, ValueEventArgs<int> e) {
            UpdateCollections();
        }

        private void UpdateCollections() {
            _ = _handler.UpdateCollections();
        }
    }
}
