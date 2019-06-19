using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Flurl.Http;
using Microsoft.Xna.Framework;

namespace Blish_HUD.BHGw2Api {

    [JsonObject]
    public class Floor : ApiItem {

        public string Id { get; set; }

        [JsonProperty(PropertyName = "Texture_Dims"), JsonConverter(typeof(Converters.PointConverter))]
        public Point TextureDimensions { get; set; }

        [JsonProperty(PropertyName = "Clamped_View"), JsonConverter(typeof(Converters.RectangleConverter))]
        public Rectangle ClampedView { get; set; }

        [JsonConverter(typeof(Converters.ChildItemConverter<Region>))]
        public List<Region> Regions { get; set; }

        public static Floor FloorFromContinentAndId(int ContinentId, int floorId) {
            using (Task<String> floorRequestTask = $"https://api.guildwars2.com/v2/continents/{ContinentId}/floors/{floorId}".GetStringAsync()) {

                try {
                    floorRequestTask.Wait(Settings.TimeoutLength);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);

                    return null;
                }

                if (!floorRequestTask.IsFaulted) {
                    if (floorRequestTask.Exception != null) {
                        Console.WriteLine("Http request failed!");

                        Console.WriteLine(
                                          string.Join(
                                                      Environment.NewLine,
                                                      floorRequestTask.Exception.InnerExceptions.Select(ie => ie.Message)
                                                     )
                                         );
                    }

                    while (!floorRequestTask.IsCompleted) {
                    }
                }

                string floorResponse = floorRequestTask.Result;

                return JsonConvert.DeserializeObject<Floor>(floorResponse, Settings.jsonSettings);
            }
        }

        public override string CacheKey() => this.Id;

    }
}
