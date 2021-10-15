using Blish_HUD.Gw2WebApi;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.Managers {
    public class Gw2ApiManager {

        private static readonly Logger Logger = Logger.GetLogger<Gw2ApiManager>();

        private const int    SUBTOKEN_LIFETIME  = 7;
        private const string SUBTOKEN_CLAIMTYPE = "permissions";

        private static readonly List<Gw2ApiManager> _apiManagers = new List<Gw2ApiManager>();

        internal static async Task RenewAllSubtokens() {
            foreach (var apiManager in _apiManagers) {
                await apiManager.RenewSubtoken();
            }
        }

        private readonly HashSet<TokenPermission> _permissions;

        private HashSet<TokenPermission> _activePermissions;

        private readonly JwtSecurityTokenHandler _subtokenHandler;

        private readonly ManagedConnection _connection;

        public event EventHandler<ValueEventArgs<IEnumerable<TokenPermission>>> SubtokenUpdated;

        public bool HasSubtoken => _connection.HasApiKey();

        public IGw2WebApiClient Gw2ApiClient => _connection.Client;

        public List<TokenPermission> Permissions => _permissions.ToList();

        private Gw2ApiManager(IEnumerable<TokenPermission> permissions, ManagedConnection moduleConnection) {
            _apiManagers.Add(this);

            _permissions       = permissions.ToHashSet();
            _activePermissions = new HashSet<TokenPermission>();
            _subtokenHandler   = new JwtSecurityTokenHandler();

            _connection = moduleConnection;
        }

        internal static Gw2ApiManager GetModuleInstance(ModuleManager module) {
            return new Gw2ApiManager(module.State.UserEnabledPermissions ?? Array.Empty<TokenPermission>(), GameService.Gw2WebApi.GetConnection(string.Empty));
        }

        internal async Task RenewSubtoken() {
            // If we have no consented permissions, we early exit.
            if (_permissions == null || !_permissions.Any()) return;

            string responseToken = string.Empty;

            try {
                responseToken = await GameService.Gw2WebApi.RequestPrivilegedSubtoken(_permissions, SUBTOKEN_LIFETIME);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to get privileged subtoken for module.");
            }

            _activePermissions.Clear();
            if (_connection.SetApiKey(responseToken) && _subtokenHandler.CanReadToken(responseToken)) {
                try {
                    var jwtToken = _subtokenHandler.ReadJwtToken(responseToken);

                    _activePermissions = jwtToken.Claims.Where(x => x.Type.Equals(SUBTOKEN_CLAIMTYPE) && Enum.TryParse(x.Value, true, out TokenPermission _))
                                                 .Select(y => (TokenPermission)Enum.Parse(typeof(TokenPermission), y.Value, true))
                                                 .ToHashSet();

                    // TODO: consider checking against the expiration claim.

                    SubtokenUpdated?.Invoke(this, new ValueEventArgs<IEnumerable<TokenPermission>>(_activePermissions));
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to parse API subtoken.");
                }
            }
        }

        [Obsolete("HavePermission is deprecated, please use HasPermission instead.", true)]
        public bool HavePermission(TokenPermission permission) => HasPermission(permission);

        [Obsolete("HavePermissions is deprecated, please use HasPermissions instead.", true)]
        public bool HavePermissions(IEnumerable<TokenPermission> permissions) => HasPermissions(permissions);

        public bool HasPermissions(IEnumerable<TokenPermission> permissions) {
            return _activePermissions.IsSupersetOf(permissions);
        }

        public bool HasPermission(TokenPermission permission) {
            return _activePermissions.Contains(permission);
        }
    }

}
