using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

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

        public static AccessTokenModel FromResponse(string json) {
            return JsonConvert.DeserializeObject<AccessTokenModel>(json);
        }
    }
}
