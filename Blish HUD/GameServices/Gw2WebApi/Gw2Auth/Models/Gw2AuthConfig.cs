namespace Blish_HUD.Gw2WebApi.Gw2Auth.Models {
    internal class Gw2AuthConfig {

        public string Gw2AuthClientId { get; private set; } = "9284be55-229e-4c90-941f-798ac8887512";

        public string Gw2AuthClientSecret { get; private set; } = "cpeJ2n40pX3zraQMFBQzI3MwYxuagGGmwY5i0TrScDie6G8YjV0kPsewDeKpuzVr";

        public string DefaultResponseType { get; private set; } = "code";

        public string DefaultScope { get; private set; } = "gw2:account gw2:characters gw2auth:verified";

        public string DefaultRedirectUri { get; private set; } = "http://127.0.0.1:8080/";

        public string DefaultPrompt { get; private set; } = "consent";

        public int StateParamLength { get; private set; } = 35;

        public string GrantTypeAuthorization { get; private set; } = "authorization_code";

        public string GrantTypeRefresh { get; private set; } = "refresh_token";

    }
}
