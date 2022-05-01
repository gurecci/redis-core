using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedisCore.Models;

namespace RedisCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private IEnumerable<Photo> Photos { get; set; }
        private bool GetPhotosError { get; set; }

        public PhotosController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IEnumerable<Photo>> OnGet()
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