using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OMM.Desktop.Data.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OMM.Desktop.Data.OmmApi
{
    public class OmmApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public static HashSet<int> AvailableMaps { get; set; } = new HashSet<int>();
        public bool HasUpdate { get; set; } = false;
        public string Version { get; } = "0.3";
        private int versionNumber = 3;
        private ISettings settings;

        public OmmApiService(IHttpClientFactory factory, ISettings settings)
        {
            _httpClientFactory = factory;
            this.settings = settings;

            if (AvailableMaps.Count == 0)
                this.GetAvailableMaps();

            this.HasUpdates();
        }

        public async Task<Either<List<MapMatch>, List<string>>> GetMapMatches(int? beatmapId, int? beatmapSetId, int count = 10)
        {
            var errors = new List<string>();

            if (beatmapId is null || beatmapSetId is null)
                errors.Add("No beatmap selected");

            if (count < 1 || count > 50)
                errors.Add("You can only get 1-50 maps");

            if (errors.Count != 0)
                return errors;

            using var client = _httpClientFactory.CreateClient("OmmApi");

            try
            {
                var matches = await client.GetFromJsonAsync<List<MapMatch>>($"api/knn/search?id={beatmapId}&count={count}");

                if (this.settings.UserSettings.HideResultWithSameBeatmapId)
                    matches.RemoveAll(match => match.BeatmapSetId == beatmapSetId);

                foreach (var map in matches)
                {
                    map.KDistance = @Math.Round(map.KDistance, 2);
                    map.CS = @Math.Round(map.CS, 1);
                    map.HP = @Math.Round(map.HP, 1);
                    map.OD = @Math.Round(map.OD, 1);
                    map.AR = @Math.Round(map.AR, 1);
                    map.ImageLink = $"https://assets.ppy.sh/beatmaps/{map.BeatmapSetId}/covers/list@2x.jpg";
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
                    case HttpStatusCode.BadGateway:
                        errors.Add("Stuff is being done on the server. Try again later.");
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


            return errors;
        }

        private void GetAvailableMaps()
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                Thread.Sleep(2000);

            var errors = new List<string>();

            using var client = _httpClientFactory.CreateClient("OmmApi");

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
                    case HttpStatusCode.BadGateway:
                        errors.Add("Stuff is being done on the server. Try again later.");
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

        private void HasUpdates()
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                Thread.Sleep(2000);

            var errors = new List<string>();

            using var client = _httpClientFactory.CreateClient("OmmApi");

            try
            {
                var version = JsonConvert.DeserializeObject<string>(client.GetStringAsync("/version").Result);
                version = version.Replace(".", "");

                if (!int.TryParse(version, out int versionNumber))
                    return;

                if (versionNumber > this.versionNumber)
                    this.HasUpdate = true;
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
                    case HttpStatusCode.BadGateway:
                        errors.Add("Stuff is being done on the server. Try again later.");
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
