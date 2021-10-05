using Blish_HUD.Gw2WebApi;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
namespace Blish_HUD.Modules.Managers {
    public class Gw2ApiManager {

        private const int SUBTOKEN_LIFETIME = 7;

        private static readonly List<Gw2ApiManager> _apiManagers = new List<Gw2ApiManager>();

        internal static void RenewAllSubtokens() {
            foreach (var apiManager in _apiManagers) {
                apiManager.RenewSubtoken();
            }
        }

        private readonly HashSet<TokenPermission> _permissions;

        private HashSet<TokenPermission> _activePermissions;

        private JwtSecurityTokenHandler _subtokenHandler;

        private ManagedConnection _connection;

        public event EventHandler<ValueEventArgs<string>> SubtokenUpdated;

        public IGw2WebApiClient Gw2ApiClient => _connection.Client;

        public List<TokenPermission> Permissions => _permissions.ToList();

        private Gw2ApiManager(IEnumerable<TokenPermission> permissions) {
            _apiManagers.Add(this);

            _permissions       = permissions.ToHashSet();
            _activePermissions = new HashSet<TokenPermission>();
            _subtokenHandler   = new JwtSecurityTokenHandler();

            RenewSubtoken();
        }

        internal static Gw2ApiManager GetModuleInstance(ModuleManager module) {
            return new Gw2ApiManager(module.State.UserEnabledPermissions ?? new TokenPermission[0]);
        }

        private void RenewSubtoken() {
            // Default to anonymous if permissions aren't requested
            // and while we wait for subtoken response.
            _connection = GameService.Gw2WebApi.AnonymousConnection;

            if (_permissions == null || !_permissions.Any()) return;

            GameService.Gw2WebApi.RequestPrivilegedSubtoken(_permissions, SUBTOKEN_LIFETIME)
                       .ContinueWith(subtokenTask => {
                            if (_connection.SetApiKey(subtokenTask.Result)) {
                                var jwtToken = _subtokenHandler.ReadJwtToken(subtokenTask.Result);
                                _activePermissions = jwtToken.Claims.Where(x => x.Type.Equals("permissions") && Enum.TryParse(x.Value, true, out TokenPermission _))
                                                                    .Select(y => (TokenPermission)Enum.Parse(typeof(TokenPermission), y.Value, true)).ToHashSet();
                                SubtokenUpdated?.Invoke(this, new ValueEventArgs<string>(subtokenTask.Result));
                            }
                        });
        }

        [Obsolete("HavePermission is deprecated, please use HasPermission instead.")]
        public bool HavePermission(TokenPermission permission) {
            return _permissions.Contains(permission);
        }

        [Obsolete("HavePermissions is deprecated, please use HasPermissions instead.")]
        public bool HavePermissions(IEnumerable<TokenPermission> permissions) {
            return _permissions.IsSupersetOf(permissions);
        }

        public bool HasPermissions(IEnumerable<TokenPermission> permissions) {
            return _activePermissions.IsSupersetOf(permissions);
        }

        public bool HasPermission(TokenPermission permission) {
            return _activePermissions.Contains(permission);
        }
    }

}
