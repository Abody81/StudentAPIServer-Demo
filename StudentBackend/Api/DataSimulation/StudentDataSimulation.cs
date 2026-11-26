using StudentApi.Backend.Models;

namespace StudentApi.DataSimulation
{
    public static class StudentDataSimulation
    {
        // Fixed, recognizable ids for the demo accounts: stable across restarts
        // and easy to follow in logs/audit lines. Real accounts (AddStudent)
        // still get Guid.NewGuid().
        public static readonly List<Student> StudentsList = new List<Student>
        {
            new Student
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "Ali Ahmed",
                Age = 20,
                Grade = 88,
                Email = "ali.ahmed@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password1"),
                Role = "Student"
            },
            new Student
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "Fadi Khail",
                Age = 22,
                Grade = 77,
                Email = "fadi.khail@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password2"),
                Role = "Student"
            },
            new Student
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "Ola Jaber",
                Age = 21,
                Grade = 66,
                Email = "ola.jaber@student.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password3"),
                Role = "Student"
            },
            new Student
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "Alia Maher",
                Age = 19,
                Grade = 44,
                Email = "alia.maher@admin.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Alia123"),
                Role = "Admin"
            },
            new Student
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Name = "Student Student",
                Age = 19,
                Grade = 44,
                Email = "S@g.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("s123"),
                Role = "Student"
            },
            new Student
            {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                Name = "Admin Admin",
                Age = 19,
                Grade = 44,
                Email = "A@g.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            }
        };
    }
}
