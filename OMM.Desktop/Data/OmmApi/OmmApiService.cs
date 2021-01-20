﻿using Microsoft.Extensions.Options;
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

        public async Task<Either<List<MapMatch>, List<string>>> GetMapMatches(int? beatmapId, int count = 10)
        {
            var errors = new List<string>();

            if (beatmapId is null)
                errors.Add("No beatmap selected");

            if (count < 1 || count > 100)
                errors.Add("You can get form 1 to 100 maps");

            if (errors.Count != 0)
                return errors;

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
                errors.Add("An error occurred.");
            }
            catch (NotSupportedException) // When content type is not valid
            {
                errors.Add("The content type is not supported.");
            }
            catch (Exception) // Invalid JSON
            {
                errors.Add("Oopsie woopsie Xarib made an oopsie. (Invalid JSON)");
            }

            return errors;
        }
    }
}
