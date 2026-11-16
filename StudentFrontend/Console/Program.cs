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

// Signs in with the given credentials. Returns the login data, or null on failure
// (wrong credentials, lost connection or timeout).
async Task<LoginResponse?> SignInAsync(string email, string password)
{
    try
    {
        using var signInResponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));

        if (!signInResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"\nLogin failed: {signInResponse.StatusCode}");
            var error = await signInResponse.Content.ReadAsStringAsync();
            Console.WriteLine(error);
            return null;
        }

        return await signInResponse.Content.ReadFromJsonAsync<LoginResponse>();
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("\nLost connection to the API server.");
        return null;
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("\nThe API server stopped responding (timeout).");
        return null;
    }
}

LoginResponse? loginData;

#if DEBUG
while (true)
{
    Console.WriteLine("Development mode: which account do you want to sign in to?");
    Console.WriteLine("1 = Admin account");
    Console.WriteLine("2 = Student account");
    Console.WriteLine("3 = Sign in manually");

    var devChoice = Console.ReadLine();

    string? email = null;
    string? password = null;

    if (devChoice == "1")
    {
        email = "A@g.com";
        password = "admin123";
    }

    if (devChoice == "2")
    {
        email = "S@g.com";
        password = "s123";
    }

    if (devChoice == "3")
    {
        Console.Write("Email: ");
        email = Console.ReadLine();

        Console.Write("Password: ");
        password = Console.ReadLine();
    }

    if (email is null || password is null)
    {
        Console.WriteLine("Invalid choice. Please enter 1, 2 or 3.");
        continue;
    }

    loginData = await SignInAsync(email, password);
    if (loginData is not null)
        break;
}
#else
while (true)
{
    Console.Write("Email: ");
    var email = Console.ReadLine();

    Console.Write("Password: ");
    var password = Console.ReadLine();

    if (email is null || password is null)
        continue;

    loginData = await SignInAsync(email, password);
    if (loginData is not null)
        break;
}
#endif

var token = loginData!.AccessToken;

Console.WriteLine("\nLogin successful.");
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
        try
        {
            await AddStudentAsync();
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
        try
        {
            await UpdateStudentAsync();
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

void PrintCancelled(string operation) =>
    Console.WriteLine($"\n{operation} cancelled.");

// Reads the role choice; returns null when the user cancels.
string? ReadRoleOrCancel()
{
    while (true)
    {
        var roleChoice = ReadLineOrCancel("Role (1 = Student, 2 = Admin): ");

        if (roleChoice is null)
            return null;

        if (roleChoice == "1")
            return "Student";

        if (roleChoice == "2")
            return "Admin";

        Console.WriteLine("Please enter 1 for Student or 2 for Admin.");
    }
}

// Reads a line of input; returns null when the user types "c" or "cancel" to abort the current operation.
string? ReadLineOrCancel(string prompt)
{
    Console.Write(prompt);
    var input = Console.ReadLine();

    return input is not null &&
           (input.Trim().Equals("c", StringComparison.OrdinalIgnoreCase) ||
            input.Trim().Equals("cancel", StringComparison.OrdinalIgnoreCase))
        ? null
        : input;
}

// Reads an integer with a minimum allowed value; returns null when the user cancels.
int? ReadIntOrCancel(string prompt, int min)
{
    while (true)
    {
        var input = ReadLineOrCancel(prompt);

        if (input is null)
            return null;

        if (int.TryParse(input, out var value) && value >= min)
            return value;

        Console.WriteLine($"Please enter a valid number greater than or equal to {min}.");
    }
}

async Task AddStudentAsync()
{
    Console.WriteLine("\n(Type 'c' at any prompt to cancel)");

    var name = ReadLineOrCancel("Name: ");
    if (name is null) { PrintCancelled("Add student"); return; }

    var age = ReadIntOrCancel("Age: ", 0);
    if (age is null) { PrintCancelled("Add student"); return; }

    var grade = ReadIntOrCancel("Grade: ", 0);
    if (grade is null) { PrintCancelled("Add student"); return; }

    var email = ReadLineOrCancel("Email: ");
    if (email is null) { PrintCancelled("Add student"); return; }

    var password = ReadLineOrCancel("Password: ");
    if (password is null) { PrintCancelled("Add student"); return; }

    var role = ReadRoleOrCancel();
    if (role is null) { PrintCancelled("Add student"); return; }

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    try
    {
        using var addResponse = await client.PostAsJsonAsync("api/Students",
            new CreateStudentRequest
            {
                Name = name,
                Age = age.Value,
                Grade = grade.Value,
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
}

async Task UpdateStudentAsync()
{
    Console.WriteLine("\n(Type 'c' at any prompt to cancel)");

    var updateId = ReadIntOrCancel("Student ID to update: ", 1);
    if (updateId is null) { PrintCancelled("Update student"); return; }

    var newName = ReadLineOrCancel("Name: ");
    if (newName is null) { PrintCancelled("Update student"); return; }

    var newAge = ReadIntOrCancel("Age: ", 0);
    if (newAge is null) { PrintCancelled("Update student"); return; }

    var newGrade = ReadIntOrCancel("Grade: ", 0);
    if (newGrade is null) { PrintCancelled("Update student"); return; }

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    try
    {
        using var updateResponse = await client.PutAsJsonAsync($"api/Students/{updateId.Value}",
            new UpdateStudentRequest
            {
                Name = newName,
                Age = newAge.Value,
                Grade = newGrade.Value
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
}

}
