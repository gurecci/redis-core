using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RedisCore.Extensions;
using RedisCore.Models;

namespace RedisCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IDistributedCache _cache;
        private IEnumerable<Photo> Photos { get; set; }
        private bool GetPhotosError { get; set; }

        public PhotosController(IHttpClientFactory clientFactory, IDistributedCache cache)
        {
            _clientFactory = clientFactory;
            _cache = cache;
        }

        [HttpGet("GetPhotos")]
        public async Task<IEnumerable<Photo>> GetPhotos()
        {
            const string recordKey = "PhotosHolder_";

            var photos = await _cache.GetRecordAsync<IEnumerable<Photo>>(recordKey);

            if (photos is null)
            {
                var data = await GetPhotosAsync();

                await _cache.SetRecordAsync(recordKey, data);

                return data;
            }

            return photos;
        }

        public async Task<IEnumerable<Photo>> GetPhotosAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://jsonplaceholder.typicode.com/photos");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                Photos = await JsonSerializer.DeserializeAsync<IEnumerable<Photo>>(responseStream);
            }
            else
            {
                GetPhotosError = true;
                Photos = Array.Empty<Photo>();
            }

            return Photos;
        }
    }
}