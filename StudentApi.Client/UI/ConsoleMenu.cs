using StudentApi.Client.Services.Interfaces;
using StudentApi.Client.State;
using StudentApi.Shared;
using StudentApi.Shared.Requests;

namespace StudentApi.Client.UI;

public class ConsoleMenu : IConsoleMenu
{
    private readonly AppState _state;
    private readonly IAuthService _authService;
    private readonly ITokenRefreshService _refreshService;
    private readonly IStudentReadService _readService;
    private readonly IStudentWriteService _writeService;

    public ConsoleMenu(
        AppState state,
        IAuthService authService,
        ITokenRefreshService refreshService,
        IStudentReadService readService,
        IStudentWriteService writeService)
    {
        _state = state;
        _authService = authService;
        _refreshService = refreshService;
        _readService = readService;
        _writeService = writeService;
    }

    public async Task RunAsync()
    {
        PrintHeader("Welcome");
        Console.WriteLine("1. Login");
        Console.WriteLine("0. Exit");
        Console.Write("\nChoice: ");

        var choice = Console.ReadLine();

        if (choice != "1")
        {
            PrintInfo("Goodbye!");
            return;
        }

        if (!await LoginAsync())
        {
            return;
        }

        await ShowMainMenuAsync();
    }

    private async Task<bool> LoginAsync()
    {
        var success = false;

        while (success == false)
        {
            Console.Write("Email: ");
            var email = Console.ReadLine()!;

            Console.Write("Password: ");
            var password = ReadPassword();
            Console.WriteLine();

            PrintInfo("Logging in...");

            success = await _authService.LoginAsync(email, password);

            if (!success)
            {
                PrintError("Login failed. Invalid credentials.");
            }
        }

        PrintSuccess($"Login successful! User ID: {_state.UserId}");
        return true;
    }

    private async Task ShowMainMenuAsync()
    {
        bool running = true;

        while (running)
        {
            PrintHeader($"Main Menu | User ID: {_state.UserId}");
            PrintTokenStatus();

            Console.WriteLine("1. View My Profile");
            Console.WriteLine("2. View All Students (Admin only)");
            Console.WriteLine("3. View Passed Students");
            Console.WriteLine("4. View Average Grade");
            Console.WriteLine("5. Add Student (Admin only)");
            Console.WriteLine("6. Update Student");
            Console.WriteLine("7. Delete Student");
            Console.WriteLine("8. Refresh Token Manually");
            Console.WriteLine("9. Logout");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoice: ");

            var choice = Console.ReadLine();

            try
            {
                running = await HandleChoiceAsync(choice);
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                PrintInfo("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }

    private async Task<bool> HandleChoiceAsync(string? choice)
    {
        switch (choice)
        {
            case "1":
                await ViewMyProfileAsync();
                return true;
            case "2":
                await ViewAllStudentsAsync();
                return true;
            case "3":
                await ViewPassedStudentsAsync();
                return true;
            case "4":
                await ViewAverageGradeAsync();
                return true;
            case "5":
                await AddStudentAsync();
                return true;
            case "6":
                await UpdateStudentAsync();
                return true;
            case "7":
                await DeleteStudentAsync();
                return true;
            case "8":
                await RefreshTokenManualAsync();
                return true;
            case "9":
                await LogoutAsync();
                return false;
            case "0":
                PrintInfo("Goodbye!");
                return false;
            default:
                PrintError("Invalid choice.");
                Console.ReadKey(true);
                return true;
        }
    }

    private async Task ViewMyProfileAsync()
    {
        var student = await _readService.GetByIdAsync(_state.UserId);
        PrintStudent(student);
        WaitForKey();
    }

    private async Task ViewAllStudentsAsync()
    {
        var students = await _readService.GetAllAsync();
        PrintStudents(students);
        WaitForKey();
    }

    private async Task ViewPassedStudentsAsync()
    {
        var students = await _readService.GetPassedAsync();
        PrintStudents(students);
        WaitForKey();
    }

    private async Task ViewAverageGradeAsync()
    {
        var average = await _readService.GetAverageGradeAsync();

        if (average.HasValue)
        {
            PrintSuccess($"Average Grade: {average.Value:F2}");
        }
        else
        {
            PrintError("Failed to load average grade.");
        }

        WaitForKey();
    }

    private async Task AddStudentAsync()
    {
        var request = ReadCreateStudentRequest();
        var student = await _writeService.AddAsync(request);

        if (student is not null)
        {
            PrintSuccess("Student added successfully:");
            PrintStudent(student);
        }
        else
        {
            PrintError("Failed to add student.");
        }

        WaitForKey();
    }

    private async Task UpdateStudentAsync()
    {
        Console.Write("Student ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            PrintError("Invalid ID.");
            WaitForKey();
            return;
        }

        var existing = await _readService.GetByIdAsync(id);

        if (existing is null)
        {
            PrintError("Student not found.");
            WaitForKey();
            return;
        }

        Console.Write($"Name ({existing.Name}): ");
        var name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name)) existing.Name = name;

        Console.Write($"Age ({existing.Age}): ");
        if (int.TryParse(Console.ReadLine(), out var age)) existing.Age = age;

        Console.Write($"Grade ({existing.Grade}): ");
        if (int.TryParse(Console.ReadLine(), out var grade)) existing.Grade = grade;

        var updated = await _writeService.UpdateAsync(id, existing);

        if (updated is not null)
        {
            PrintSuccess("Student updated successfully:");
            PrintStudent(updated);
        }
        else
        {
            PrintError("Failed to update student.");
        }

        WaitForKey();
    }

    private async Task DeleteStudentAsync()
    {
        Console.Write("Student ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            PrintError("Invalid ID.");
            WaitForKey();
            return;
        }

        Console.Write("Are you sure? (y/n): ");
        var confirm = Console.ReadLine();

        if (confirm?.ToLower() != "y")
        {
            PrintInfo("Cancelled.");
            WaitForKey();
            return;
        }

        var success = await _writeService.DeleteAsync(id);

        if (success)
        {
            PrintSuccess("Student deleted successfully.");
        }
        else
        {
            PrintError("Failed to delete student.");
        }

        WaitForKey();
    }

    private async Task RefreshTokenManualAsync()
    {
        var success = await _refreshService.RefreshAsync();

        if (success)
        {
            PrintSuccess("Token refreshed successfully.");
        }
        else
        {
            PrintError("Manual refresh failed.");
        }

        WaitForKey();
    }

    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        PrintSuccess("Logged out successfully.");
    }

    private static CreateStudentRequest ReadCreateStudentRequest()
    {
        Console.Write("Name: ");
        var name = Console.ReadLine()!;

        Console.Write("Age: ");
        _ = int.TryParse(Console.ReadLine(), out var age);

        Console.Write("Grade: ");
        _ = int.TryParse(Console.ReadLine(), out var grade);

        Console.Write("Email: ");
        var email = Console.ReadLine()!;

        Console.Write("Password: ");
        var password = ReadPassword();
        Console.WriteLine();

        Console.Write("Role (Student/Admin): ");
        var role = Console.ReadLine()!;

        return new CreateStudentRequest
        {
            Name = name,
            Age = age,
            Grade = grade,
            Email = email,
            Password = password,
            Role = role
        };
    }

    private static void PrintStudent(StudentDto? student)
    {
        if (student is null)
        {
            PrintWarning("No data.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"ID:    {student.Id}");
        Console.WriteLine($"Name:  {student.Name}");
        Console.WriteLine($"Age:   {student.Age}");
        Console.WriteLine($"Grade: {student.Grade}");
        Console.WriteLine($"Email: {student.Email}");
        Console.WriteLine($"Role:  {student.Role}");
        Console.WriteLine();
    }

    private static void PrintStudents(List<StudentDto>? students)
    {
        if (students is null || students.Count == 0)
        {
            PrintWarning("No students found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Students count: {students.Count}\n");

        foreach (var s in students)
        {
            Console.WriteLine($"- ID: {s.Id} | {s.Name} | Age: {s.Age} | Grade: {s.Grade} | Role: {s.Role}");
        }

        Console.WriteLine();
    }

    private void PrintTokenStatus()
    {
        var remaining = _state.RemainingTokenTime;

        Console.ForegroundColor = remaining.TotalMinutes < 2 ? ConsoleColor.Red : ConsoleColor.Green;
        Console.WriteLine($"Access Token expires in: {remaining:mm\\:ss}");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void PrintHeader(string title)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {title} ===");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void WaitForKey()
    {
        PrintInfo("Press any key to continue...");
        Console.ReadKey(true);
    }

    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        return password;
    }
}
