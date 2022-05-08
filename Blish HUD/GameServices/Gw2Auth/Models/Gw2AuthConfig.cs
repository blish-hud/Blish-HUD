namespace Blish_HUD.GameServices.Gw2Auth.Models {
    internal class Gw2AuthConfig {

        public string Gw2AuthClientId { get; set; } = "dae449fa-b333-4bea-9562-3f1bcd827af7";

        public string Gw2AuthClientSecret { get; set; } = "XAl8p7ZR1AZszHQpEoM3iCSPw88HwdLtjcMzgrvBPdISvV6qHOoj272GNvdCNahN";

        public string DefaultResponseType { get; set; } = "code";

        public string DefaultScope { get; set; } = "gw2:account gw2:characters gw2auth:verified";

        public string DefaultRedirectUri { get; set; } = "http://127.0.0.1:8080/";

        public string DefaultPrompt { get; set; } = "consent";

        public int StateParamLength { get; set; } = 35;

        public string GrantTypeAuthorization { get; set; } = "authorization_code";

        public string GrantTypeRefresh { get; set; } = "refresh_token";

    }
}
