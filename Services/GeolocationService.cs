using Backend.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Backend.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeolocationService> _logger;

        public GeolocationService(HttpClient httpClient, ILogger<GeolocationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> GetCountryByIpAsync(string ipAddress)
        {
            try
            {

                var response = await _httpClient.GetStringAsync($"https://ipinfo.io/{ipAddress}/json");
                dynamic info = JsonConvert.DeserializeObject(response);
                return info.country;
            }
            catch (Exception ex)
            {
                return "AU";
            }

        }
    }
}