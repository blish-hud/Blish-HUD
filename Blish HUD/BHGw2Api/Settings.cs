using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.BHGw2Api {

    public enum Culture {
        en, // English (default)
        de, // German
        es, // Spanish
        fr  // French
    }

    public static class Settings {

        public static int TimeoutLength { get; set; } = 3000;

        public static JsonSerializerSettings jsonSettings = new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

        public static Culture CurrentCulture { get; set; } = Culture.en;

        private static string _cacheDir = "cache";
        public static string CacheLocation => Path.Combine(GameService.FileSrv.BasePath, _cacheDir);

        public static void Load() {
            // Make sure the directory is there for us
            Directory.CreateDirectory(CacheLocation);

            Flurl.Http.FlurlHttp.Configure(settings => {
                settings.JsonSerializer = new Flurl.Http.Configuration.NewtonsoftJsonSerializer(jsonSettings);
            });

            RenderService.Load();
            Meta.Load();
        }

    }
}
