using System.Net;
using System.Net.Http.Headers;
using StudentApi.Shared;
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

while (true)
{
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

Console.WriteLine("Write 1 to get profile: ");
Console.WriteLine("Write 2 to get all student profiles (admin only): ");
Console.WriteLine("Write 3 to renew access token: ");

while (true)
{
    Console.Write("Your chose: ");
    string? Input = Console.ReadLine();

    if (Input == "1")
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var profileResponse = await client.GetAsync($"api/Students/{loginData.UserId}");

            if (profileResponse.IsSuccessStatusCode)
            {
                var profile = await profileResponse.Content.ReadFromJsonAsync<StudentDto>();
                Console.WriteLine("\n--- Student Profile ---");
                Console.WriteLine($"ID:     {profile!.Id}");
                Console.WriteLine($"Name:   {profile.Name}");
                Console.WriteLine($"Age:    {profile.Age}");
                Console.WriteLine($"Grade:  {profile.Grade}");
                Console.WriteLine($"Email:  {profile.Email}");
                Console.WriteLine($"Role:   {profile.Role}");
            }
            else
            {
                Console.WriteLine($"\nFailed to get profile: {profileResponse.StatusCode}");
                var error = await profileResponse.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("\nLost connection to the API server.");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("\nThe API server stopped responding (timeout).");
        }

        Console.WriteLine();
    }

    if (Input == "2")
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var allResponse = await client.GetAsync("api/Students/All");

            if (allResponse.IsSuccessStatusCode)
            {
                var students = await allResponse.Content.ReadFromJsonAsync<List<StudentDto>>();

                Console.WriteLine("\n--- All Student Profiles ---");
                foreach (var student in students!)
                {
                    Console.WriteLine($"ID:     {student.Id}");
                    Console.WriteLine($"Name:   {student.Name}");
                    Console.WriteLine($"Age:    {student.Age}");
                    Console.WriteLine($"Grade:  {student.Grade}");
                    Console.WriteLine($"Email:  {student.Email}");
                    Console.WriteLine($"Role:   {student.Role}");
                    Console.WriteLine("---------------------------");
                }
            }
            else if (allResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAccess denied: this option is for admins only. " +
                                  "Log in with an admin account to view all student profiles.");
                Console.ResetColor();
            }
            else if (allResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("\nYour session is not authenticated. Please renew your access token (option 3) or log in again.");
            }
            else
            {
                Console.WriteLine($"\nFailed to get students: {allResponse.StatusCode}");
                var error = await allResponse.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("\nLost connection to the API server.");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("\nThe API server stopped responding (timeout).");
        }

        Console.WriteLine();
    }

    if (Input == "3")
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var refreshResponse = await client.PostAsJsonAsync("api/auth/refresh",
                new RefreshRequest(loginData!.UserId, loginData.RefreshToken));

            if (refreshResponse.IsSuccessStatusCode)
            {
                loginData = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
                token = loginData!.AccessToken;

                Console.WriteLine("Refresh successfully");
                Console.WriteLine($"New access token: {loginData!.AccessToken[..40]}");
                Console.WriteLine($"New refresh token: {loginData!.RefreshToken[..40]}");
            }
            else
            {
                Console.WriteLine($"\nFailed to refresh token: {refreshResponse.StatusCode}");
                var error = await refreshResponse.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("\nLost connection to the API server.");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("\nThe API server stopped responding (timeout).");
        }

        Console.WriteLine();
    }

}
