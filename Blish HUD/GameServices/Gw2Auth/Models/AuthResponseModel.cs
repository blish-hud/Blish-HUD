using System.Collections.Specialized;

namespace Blish_HUD.GameServices.Gw2Auth.Models {
    internal class AuthResponseModel {
        /// <summary>
        /// A short-lived one time code which can be used to retrieve the initial Refresh- and Access-Token.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// A randomly generated key.
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// An OAuth2 Error-Code.
        /// </summary>
        /// <example>access_denied</example>
        public string Error { get; private set; }

        /// <summary>
        /// A detailed description of <see cref="Error"/>.
        /// </summary>
        /// <example>The user has denied your application access.</example>
        public string ErrorDescription { get; private set; }

        public AuthResponseModel() { }

        public bool IsSuccess() {
            return !string.IsNullOrEmpty(this.Code) && !string.IsNullOrEmpty(this.State);
        }

        public bool IsError() {
            return !string.IsNullOrEmpty(this.Error);
        }

        public static AuthResponseModel FromQuery(NameValueCollection queryString) {
            return new AuthResponseModel {
                Code             = queryString["code"],
                State            = queryString["state"],
                Error            = queryString["error"],
                ErrorDescription = queryString["error_description"]
            };
        }
    }
}
