using Microsoft.Extensions.Options;
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

        public OmmApiService(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;
        }

        public async Task<List<MapMatch>> GetMapMatches(int? beatmapId, int count = 10)
        {
            if (beatmapId is null)
                beatmapId = 2142695; //TODO change to null

            var client = _httpClientFactory.CreateClient("OmmApi");

            try
            {
                count++;
                var matches = await client.GetFromJsonAsync<List<MapMatch>>($"api/knn/ranked?id={beatmapId}&count={count}");
                matches.RemoveAll(match => match.KDistance == 0);
                return matches;
            }
            catch (HttpRequestException) // Non success
            {
                Console.WriteLine("An error occurred.");
            }
            catch (NotSupportedException) // When content type is not valid
            {
                Console.WriteLine("The content type is not supported.");
            }
            catch (Exception) // Invalid JSON
            {
                Console.WriteLine("Oopsie woopsie Xarib made an oopsie. (Invalid JSON)");
            }

            return null;
        }
    }
}
