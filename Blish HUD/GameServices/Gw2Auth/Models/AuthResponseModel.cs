using System.Collections.Specialized;

namespace Blish_HUD.GameServices.Gw2Auth.Models {
    internal class AuthResponseModel {
        public string Code { get; private set; }

        public string State { get; private set; }

        public string Error { get; private set; }

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
