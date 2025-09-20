using Med.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Med.Services
{
    public interface IApiService
    {
        HttpClient CreateClient();
        Task<HttpClient> CreateAuthenticatedClient(HttpContext httpContext);

        string BaseUrl { get; }
    }

    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public ApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = options.Value;
        }

        public string BaseUrl => _apiSettings.BaseUrl;

        public HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient();
        }

        public async Task<HttpClient> CreateAuthenticatedClient(HttpContext httpContext)
        {
            var token = httpContext.Request.Cookies["jwt"];
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }
    }
}
