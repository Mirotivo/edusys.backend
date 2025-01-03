using Backend.Interfaces;
using Newtonsoft.Json;

namespace Backend.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;

        public GeolocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetCountryByIpAsync(string ipAddress)
        {
            var response = await _httpClient.GetStringAsync($"https://ipinfo.io/{ipAddress}/json");
            dynamic info = JsonConvert.DeserializeObject(response);
            return info.country;
        }
    }
}
