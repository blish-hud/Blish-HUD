using Newtonsoft.Json;

namespace Blish_HUD.GameServices.Gw2Auth.Models {

    public class JwtSubtokenModel {

        /// <summary>
        /// The name of the GW2-Account, freely chosen by the user at GW2Auth
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The GW2-API-Subtoken, valid for exactly the same time as the Access-Token itself. Use this to perform GW2-API-Requests. Only present if <see cref="Error"/> is not present.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// An error description if, for any reason, no subtoken could be retrieved for this GW2-Account. Only present if <see cref="Token"/> is not present.
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }

        /// <summary>
        /// Only present if the scope <c>gw2auth:verified</c> was requested (and authorized).
        /// </summary>
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        public JwtSubtokenModel() { }

        public JwtSubtokenModel(string name, string token, string error, bool verified) {
            this.Name     = name;
            this.Token    = token;
            this.Error    = error;
            this.Verified = verified;
        }

        public bool IsError() {
            return !string.IsNullOrEmpty(this.Error);
        }
    }
}
