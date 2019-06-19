using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using System.IO;
using System.Text.RegularExpressions;

namespace Blish_HUD.BHGw2Api {
    public static class RenderService {

        public enum RenderServiceFileFormat {
            JPG,
            PNG
        }

        private const string IMAGE_CACHE = @"images";

        private static Dictionary<string, Texture2D> TextureCache;

        private static Regex knownUrlParser;

        private static string ImageLocation => Path.Combine(Settings.CacheLocation, IMAGE_CACHE);

        public static void Load() {
            TextureCache = new Dictionary<string, Texture2D>();

            // Make sure directory is available to us
            Directory.CreateDirectory(ImageLocation);

            foreach (string cachedImage in Directory.GetFiles(ImageLocation)) {
                string id = Path.GetFileNameWithoutExtension(cachedImage);
                TextureCache.Add(id, TextureFromFile(cachedImage));
            }

            knownUrlParser = new Regex(@"\/(?<signature>[A-Z0-9]+)\/(?<file_id>[0-9]+)\.(?<format>...)", RegexOptions.Compiled);
        }

        private static Texture2D TextureFromFile(string filepath) {
            if (File.Exists(filepath)) {
                using (var fileStream = new FileStream(filepath, FileMode.Open)) {
                    return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, fileStream);
                }
            } else return null;
        }

        public static Texture2D GetTexture(string knownUrl) {
            var sigMatch = knownUrlParser.Match(knownUrl);

            string signature = sigMatch.Groups["signature"].Value;
            string fileId = sigMatch.Groups["file_id"].Value;
            var format = (RenderServiceFileFormat) Enum.Parse(typeof(RenderServiceFileFormat), sigMatch.Groups["format"].Value, true);

            return GetTexture(signature, fileId, format);
        }

        public static Texture2D GetTexture(string signature, string fileId, RenderServiceFileFormat format) {
            if (!TextureCache.ContainsKey(fileId)) {
                Task<string> renderServiceRequestTask = $"https://render.guildwars2.com/file/{signature}/{fileId}.{format.ToString().ToLower()}".DownloadFileAsync(ImageLocation);

                renderServiceRequestTask.Wait(Settings.TimeoutLength);

                string texturePath = renderServiceRequestTask.Result;
                TextureCache.Add(fileId, TextureFromFile(texturePath));

                Console.WriteLine($"Had to download {texturePath}.");

                renderServiceRequestTask.Dispose();
            }
            return TextureCache[fileId]; 
        }

    }
}
