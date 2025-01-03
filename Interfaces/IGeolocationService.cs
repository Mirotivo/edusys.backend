namespace Backend.Interfaces
{
    public interface IGeolocationService
    {
        Task<string?> GetCountryByIpAsync(string ipAddress);
    }
}
