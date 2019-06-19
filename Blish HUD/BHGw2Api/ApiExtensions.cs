using Flurl;

namespace Blish_HUD.BHGw2Api {
    public static class ApiExtensions {

        public static Url WithCulture(this Url url) {
            return url.SetQueryParam("lang", Settings.CurrentCulture.ToString());
        }

        public static Url WithCulture(this string url) {
            return new Url(url).WithCulture();
        }

        public static Url WithEndpoint(this Url url, Url endpoint) {
            return url.AppendPathSegment(endpoint);
        }

        public static Url WithEndpoint(this string url, string endpoint) {
            return new Url(url).WithEndpoint(endpoint);
        }

        public static Url ById(this Url endpoint, string id) {
            return endpoint.AppendPathSegment(id);
        }

        public static Url ById(this string endpoint, string id) {
            return new Url(endpoint).ById(id);
        }

    }
}
