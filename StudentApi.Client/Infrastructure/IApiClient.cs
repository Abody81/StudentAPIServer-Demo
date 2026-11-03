namespace StudentApi.Client.Infrastructure;

public interface IApiClient
{
    Task<HttpResponseMessage> GetAsync(string url);
    Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data);
    Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T data);
    Task<HttpResponseMessage> DeleteAsync(string url);
}
