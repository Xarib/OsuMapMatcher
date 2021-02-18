using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OMM.Desktop.Data.OmmApi
{
    public class OmmApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public static HashSet<int> AvailableMaps { get; set; } = new HashSet<int>();

        public OmmApiService(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;

            if (AvailableMaps.Count == 0)
                this.GetAvailableMaps();
        }

        public async Task<Either<List<MapMatch>, List<string>>> GetMapMatches(int? beatmapId, int count = 10)
        {
            var errors = new List<string>();

            if (beatmapId is null)
                errors.Add("No beatmap selected");

            if (count < 1 || count > 50)
                errors.Add("You can retreive 1-50 maps");

            if (errors.Count != 0)
                return errors;

            using (var client = _httpClientFactory.CreateClient("OmmApi"))
            {
                try
                {
                    count++;
                    var matches = await client.GetFromJsonAsync<List<MapMatch>>($"api/knn/search?id={beatmapId}&count={count}");

                    matches.RemoveAll(match => match.KDistance < 0.001);

                    foreach (var map in matches)
                    {
                        map.KDistance = @Math.Round(map.KDistance, 2);
                        map.CS = @Math.Round(map.CS, 1);
                        map.HP = @Math.Round(map.HP, 1);
                        map.OD = @Math.Round(map.OD, 1);
                        map.AR = @Math.Round(map.AR, 1);
                        map.ImageLink = $"https://assets.ppy.sh/beatmaps/{map.BeatmapSetId}/covers/list@2x.jpg";
                        //map.OsuDirectLink = $"osu://b/{map.BeatmapId}";
                        //map.MapLink = $"https://osu.ppy.sh/b/{map.BeatmapId}";
                        map.TrackPreview = $"https://b.ppy.sh/preview/{map.BeatmapSetId}.mp3";
                    }

                    return matches;
                }
                catch (HttpRequestException e) // Non success
                {
                    switch (e.StatusCode)
                    {
                        case HttpStatusCode.TooManyRequests:
                            errors.Add("You can't spam this button");
                            return errors;
                        default:
                            errors.Add("An error occurred.");
                            break;
                    }
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    errors.Add("The content type is not supported.");
                }
                catch (Exception) // Invalid JSON
                {
                    errors.Add("Oopsie woopsie Xarib made an oopsie.");
                }
            }

            return errors;
        }

        private void GetAvailableMaps()
        {
            var errors = new List<string>();

            using (var client = _httpClientFactory.CreateClient("OmmApi"))
            {
                try
                {
                    AvailableMaps = JsonConvert.DeserializeObject<HashSet<int>>(client.GetStringAsync("api/knn/maps").Result);
                }
                catch (HttpRequestException e) // Non success
                {
                    switch (e.StatusCode)
                    {
                        case HttpStatusCode.TooManyRequests:
                            errors.Add("You have mad too many requests");
                            break;
                        case HttpStatusCode.Forbidden:
                            errors.Add("Config error. Whoops");
                            break;
                        default:
                            errors.Add("An error occurred.");
                            break;
                    }
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    errors.Add("The content type is not supported.");
                }
                catch (Exception) // Invalid JSON
                {
                    errors.Add("Oopsie woopsie Xarib made an oopsie.");
                }
            }
        }
    }
}
