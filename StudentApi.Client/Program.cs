using StudentApi.Client.Infrastructure;
using StudentApi.Client.Services;
using StudentApi.Client.Services.Interfaces;
using StudentApi.Client.State;
using StudentApi.Client.UI;

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};

using var httpClient = new HttpClient(handler);
httpClient.BaseAddress = new Uri("https://localhost:7217/");

var state = new AppState();
var refreshService = new TokenRefreshService(httpClient, state);
var apiClient = new ApiClient(httpClient, state, refreshService);
var authService = new AuthService(httpClient, state);
var readService = new StudentReadService(apiClient);
var writeService = new StudentWriteService(apiClient);

var menu = new ConsoleMenu(state, authService, refreshService, readService, writeService);

await menu.RunAsync();
