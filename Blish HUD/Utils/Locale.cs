using System.Globalization;

namespace Blish_HUD.Utils {
    public static class Locale {

        public static Gw2Sharp.WebApi.Locale GetGw2LocaleFromCurrentUICulture() {
            string currLocale = CultureInfo.CurrentUICulture.EnglishName.Split(' ')[0];

            switch (currLocale) {
                case "Chinese":
                    return Gw2Sharp.WebApi.Locale.Chinese;
                case "French":
                    return Gw2Sharp.WebApi.Locale.French;
                case "German":
                    return Gw2Sharp.WebApi.Locale.German;
                case "Korean":
                    return Gw2Sharp.WebApi.Locale.Korean;
                case "Spanish":
                    return Gw2Sharp.WebApi.Locale.Spanish;
                case "English":
                default:
                    return Gw2Sharp.WebApi.Locale.English;
            }
        }

    }
}
