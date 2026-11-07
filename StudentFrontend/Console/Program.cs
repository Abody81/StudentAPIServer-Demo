using System.IO.Compression;
using StudentApi.Shared.Requests;
using StudentApi.Shared.Responses;

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};

using var client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:7217/");
client.Timeout = TimeSpan.FromSeconds(5);

Console.WriteLine("Connecting to API...");

try
{
    using var healthResponse = await client.GetAsync("health");
    healthResponse.EnsureSuccessStatusCode();
    Console.WriteLine("API is up.\n");
}
catch (HttpRequestException)
{
    Console.WriteLine("Cannot reach the API server. Is it running? (https://localhost:7217)");
    return;
}
catch (TaskCanceledException)
{
    Console.WriteLine("The API server did not respond in time (timeout).");
    return;
}

HttpResponseMessage loginResponse;
short counter = 0;

while (true)
{
    if (counter++ >= 3)
    {
        Console.WriteLine("your tires end!");
        return;
    }

    Console.Write("Email: ");
    var email = Console.ReadLine()!;

    Console.Write("Password: ");
    var password = Console.ReadLine()!;

    try
    {
        loginResponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("\nLost connection to the API server.");
        return;
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("\nThe API server stopped responding (timeout).");
        return;
    }

    if (loginResponse.IsSuccessStatusCode)
        break;

    Console.WriteLine($"\nLogin failed: {loginResponse.StatusCode}");
    var error = await loginResponse.Content.ReadAsStringAsync();
    Console.WriteLine(error + '\n');
}

var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
var token = loginData!.AccessToken;

Console.WriteLine($"\nLogin successful.");
Console.WriteLine($"Token: {token[..40]}...\n");
