using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Sharp;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;

namespace Blish_HUD {
    internal class Gw2ApiService : GameService {

        private static Logger Logger = Logger.GetLogger<Gw2ApiService>();

        private const string GW2API_SETTINGS = "Gw2ApiConfiguration";

        public event EventHandler<EventArgs> ConnectionUpdated;

        private Gw2Client _sharedApiClient;

        public Gw2Client SharedApiClient => _sharedApiClient;

        private IConnection _internalConnection;

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            UpdateConnection();

            _sharedApiClient = new Gw2Client(_internalConnection);
            _sharedApiClient.Mumble.Update();
        }

        public void UpdateConnection() {
            _internalConnection = new Connection("",
                                                 (Locale)GameService.Overlay.UserLocale,
                                                 GameService.Gw2WebApi.GetWebCacheMethod(),
                                                 GameService.Content.GetRenderServiceCacheMethod());

            ((Connection) _internalConnection).AccessToken = "test";

            ConnectionUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

        protected override void Unload() {
            _sharedApiClient?.Dispose();
        }

    }
}
