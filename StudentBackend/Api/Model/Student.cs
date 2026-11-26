namespace StudentApi.Backend.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public int Grade { get; set; }

        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;

        public string RefreshTokenHash { get; set; } = null!;
        public DateTime RefreshTokenExpiresAt { get; set; } 
        public DateTime? RefreshTokenRevokedAt { get; set; } 

    }
}
