using Gw2Sharp.WebApi;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Gw2WebApi;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules.Managers {
    public class Gw2ApiManager {

        private readonly HashSet<TokenPermission> _permissions;

        private ManagedConnection _connection;

        private Gw2WebApiClient _gw2ApiClient;

        public IGw2WebApiV2Client Gw2ApiClient => _gw2ApiClient.V2;

        public List<TokenPermission> Permissions => _permissions.ToList();

        public Gw2ApiManager(IEnumerable<TokenPermission> permissions) {
            _permissions = permissions.ToHashSet();

            UpdateToken();

            //_gw2ApiClient = new Gw2WebApiClient(_gw2ApiConnection);
        }

        private void UpdateToken() {
            if (_permissions == null || !_permissions.Any()) {
                //_connection = new ManagedConnection();
                return;
            }

            //string apiSubtoken = GameService.Gw2WebApi.RequestSubtoken(_permissions, 7);

            //_gw2ApiConnection = new Connection(apiSubtoken, Locale.English);
        }

        public bool HavePermission(TokenPermission permission) {
            return _permissions.Contains(permission);
        }

        public bool HavePermissions(IEnumerable<TokenPermission> permissions) {
            return _permissions.IsSupersetOf(permissions);
        }

    }

}
