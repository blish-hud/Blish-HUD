using Blish_HUD.GameServices.Gw2Auth.Converter;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization;

namespace Blish_HUD.GameServices.Gw2Auth.Models {
    public enum Gw2AuthAttribute {
        [EnumMember(Value = "Unknown")]
        Unknown,
        [EnumMember(Value = "Verified")]
        Verified
    }

    public class AccessTokenModel {
        /// <summary>
        /// JWT with a JSON payload.
        /// Claims:
        ///     sub   - string   - A unique identifier for the GW2Auth-Account of the user. This value is not consistent across different clients.
        ///     scope - string[] - The list of authorized scopes
        ///     iss   - string   - A URL of the issuer which created this Access-Token
        ///     iat   - long     - UNIX-Timestamp (seconds): Timestamp at which this token was issued
        ///     exp   - long     - UNIX-Timestamp (seconds): Timestamp at which this token will expire
        ///     gw2:tokens       - Dictionary[string, object] - A JSON-Object containing all subtokens (and some additional information) for all by the user authorized GW2-Accounts.
        ///                        Key: GW2-Account-ID ; Value: <see cref="JwtSubtokenModel"/>.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The Refresh Token used to retrieve new Access Tokens (without user interaction).
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The authorized scopes.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Usually "Bearer". 
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Time at which the Access-Token expires (UTC).
        /// </summary>
        [JsonProperty("expires_in"), JsonConverter(typeof(ExpiresInSecondsConverter))]
        public DateTime ExpiresAt { get; set; }

        [JsonIgnore]
        private string _subtokenClaimType = "gw2:tokens";

        public IEnumerable<TokenPermission> GetTokenPermissions() {
            return this.Scope.Split(' ')
                       .Select(s => s.Split(':'))
                       .Where(scope => scope[0].Equals("gw2"))
                       .Select(scope => Enum.TryParse<TokenPermission>(scope[1], true, out var perm) ? perm : TokenPermission.Unknown);
        }

        public IEnumerable<Gw2AuthAttribute> GetAuthAttributes() {
            return this.Scope.Split(' ')
                       .Select(s => s.Split(':'))
                       .Where(scope => scope[0].Equals("gw2auth"))
                       .Select(scope => Enum.TryParse<Gw2AuthAttribute>(scope[1], true, out var perm) ? perm : Gw2AuthAttribute.Unknown);
        }

        /// <summary>
        /// Gets the unique persistent account <see langword="Guid"/> if it exists.
        /// </summary>
        /// <param name="id">If <see langword="true"/>: The unique persistent account <see langword="Guid"/>; Otherwise <see langword="Guid.Empty"/>.</param>
        /// <returns><see langword="True"/> if the unique persistent account <see langword="Guid"/> was retrieved; Otherwise <see langword="false"/>.</returns>
        public bool TryGetGuid(out Guid id) {
            id = Guid.Empty;
            if (string.IsNullOrEmpty(this.AccessToken)) {
                return false;
            }
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(this.AccessToken);
            return Guid.TryParse(jwtToken.Subject, out id);
        }

        /// <summary>
        /// Gets the API subtokens if any were created successfully.
        /// </summary>
        /// <param name="tokens">If <see langword="true"/>: An enumerable containing the subtokens; Otherwise <see langword="Enumerable.Empty"/>.</param>
        /// <returns><see langword="True"/> if subtokens were retrieved; Otherwise <see langword="false"/>.</returns>
        public bool TryGetSubTokens(out IEnumerable<JwtSubtokenModel> tokens) {
            tokens = Enumerable.Empty<JwtSubtokenModel>();
            if (string.IsNullOrEmpty(this.AccessToken)) {
                return false;
            }

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(this.AccessToken);
            var claim    = jwtToken.Claims.FirstOrDefault(x => x.Type.Equals(_subtokenClaimType));
            if (claim == null) {
                return false;
            }

            var users = JsonConvert.DeserializeObject<Dictionary<string, JwtSubtokenModel>>(claim.Value);
            if (users == null) {
                return false;
            }

            tokens = users.Values;
            return true;
        }
    }
}
