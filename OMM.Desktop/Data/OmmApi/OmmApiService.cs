using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    var matches = await client.GetFromJsonAsync<List<MapMatch>>($"api/knn/ranked?id={beatmapId}&count={count}");
                    matches.RemoveAll(match => match.KDistance < 0.001);
                    return matches;
                }
                catch (HttpRequestException) // Non success
                {
                    errors.Add("An error occurred.");
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    errors.Add("The content type is not supported.");
                }
                catch (Exception e) // Invalid JSON
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
                catch (HttpRequestException) // Non success
                {
                    errors.Add("A List of awailable maps could not be loaded");
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
