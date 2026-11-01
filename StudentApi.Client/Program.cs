using StudentApi.Shared;
using System.Net.Http.Headers;

Console.WriteLine("=== Student API Console Client ===\n");

// تجاوز SSL certificate في Development فقط
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};

using var client = new HttpClient(handler);
client.BaseAddress = new Uri("https://localhost:7217/");

// 1) Login
Console.Write("Email: ");
var email = Console.ReadLine()!;

Console.Write("Password: ");
var password = Console.ReadLine()!;

var loginResponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));

if (!loginResponse.IsSuccessStatusCode)
{
    Console.WriteLine($"\nLogin failed: {loginResponse.StatusCode}");
    var error = await loginResponse.Content.ReadAsStringAsync();
    Console.WriteLine(error);
    return;
}

var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
var token = loginData!.Token;

Console.WriteLine($"\nLogin successful.");
Console.WriteLine($"Token: {token[..40]}...\n");

// 2) Call protected endpoint
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var studentsResponse = await client.GetAsync("api/Students/All");

if (studentsResponse.IsSuccessStatusCode)
{
    var students = await studentsResponse.Content.ReadFromJsonAsync<List<StudentDto>>();
    Console.WriteLine($"Students count: {students?.Count}\n");

    foreach (var s in students ?? new())
    {
        Console.WriteLine($"- {s.Name} | Age: {s.Age} | Grade: {s.Grade} | Role: {s.Role}");
    }
}
else
{
    Console.WriteLine($"Failed to load students: {studentsResponse.StatusCode}");
}
