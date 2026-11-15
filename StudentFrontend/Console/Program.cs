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
Console.WriteLine("Write 3 to add new student (admin only): ");
Console.WriteLine("Write 4 to update student (admin only): ");
Console.WriteLine("Write 5 to renew access token: ");

while (true)
{
    Console.Write("Your chose: ");
    string? Input = Console.ReadLine();

    if (Input == "1")
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var profileResponse = await client.GetAsync($"api/Students/{loginData!.UserId}");

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
                Console.WriteLine("\nYour session is not authenticated. Please renew your access token (option 5) or log in again.");
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
        Console.Write("\nName: ");
        var name = Console.ReadLine()!;

        int age;
        while (true)
        {
            Console.Write("Age: ");
            if (int.TryParse(Console.ReadLine(), out age) && age >= 0)
                break;
            Console.WriteLine("Please enter a valid non-negative number.");
        }

        int grade;
        while (true)
        {
            Console.Write("Grade: ");
            if (int.TryParse(Console.ReadLine(), out grade) && grade >= 0)
                break;
            Console.WriteLine("Please enter a valid non-negative number.");
        }

        Console.Write("Email: ");
        var email = Console.ReadLine()!;

        Console.Write("Password: ");
        var password = Console.ReadLine()!;

        string role;
        while (true)
        {
            Console.Write("Role (1 = Student, 2 = Admin): ");
            var roleChoice = Console.ReadLine();

            if (roleChoice == "1")
            {
                role = "Student";
                break;
            }

            if (roleChoice == "2")
            {
                role = "Admin";
                break;
            }

            Console.WriteLine("Please enter 1 for Student or 2 for Admin.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var addResponse = await client.PostAsJsonAsync("api/Students",
                new CreateStudentRequest
                {
                    Name = name,
                    Age = age,
                    Grade = grade,
                    Email = email,
                    Password = password,
                    Role = role
                });

            if (addResponse.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nStudent added successfully!");
                Console.ResetColor();
            }
            else if (addResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAccess denied: adding students requires an admin account.");
                Console.ResetColor();
            }
            else if (addResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("\nYour session is not authenticated. Please renew your access token (option 5) or log in again.");
            }
            else
            {
                Console.WriteLine($"\nFailed to add student: {addResponse.StatusCode}");
                var error = await addResponse.Content.ReadAsStringAsync();
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

    if (Input == "4")
    {
        int updateId;
        while (true)
        {
            Console.Write("\nStudent ID to update: ");
            if (int.TryParse(Console.ReadLine(), out updateId) && updateId >= 1)
                break;
            Console.WriteLine("Please enter a valid student ID.");
        }

        Console.Write("Name: ");
        var newName = Console.ReadLine()!;

        int newAge;
        while (true)
        {
            Console.Write("Age: ");
            if (int.TryParse(Console.ReadLine(), out newAge) && newAge >= 0)
                break;
            Console.WriteLine("Please enter a valid non-negative number.");
        }

        int newGrade;
        while (true)
        {
            Console.Write("Grade: ");
            if (int.TryParse(Console.ReadLine(), out newGrade) && newGrade >= 0)
                break;
            Console.WriteLine("Please enter a valid non-negative number.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var updateResponse = await client.PutAsJsonAsync($"api/Students/{updateId}",
                new UpdateStudentRequest
                {
                    Name = newName,
                    Age = newAge,
                    Grade = newGrade
                });

            if (updateResponse.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nStudent updated successfully!");
                Console.ResetColor();
            }
            else if (updateResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAccess denied: updating students requires an admin account.");
                Console.ResetColor();
            }
            else if (updateResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("\nYour session is not authenticated. Please renew your access token (option 5) or log in again.");
            }
            else
            {
                Console.WriteLine($"\nFailed to update student: {updateResponse.StatusCode}");
                var error = await updateResponse.Content.ReadAsStringAsync();
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

    if (Input == "5")
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
