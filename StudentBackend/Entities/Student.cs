namespace StudentBackend.Entities;

/// <summary>
/// Academic profile: "what is your academic record?"
/// </summary>
public class Student
{
    /// <summary>
    /// StudentId IS the user id (User.Id) - they are the SAME number.
    /// </summary>
    public int StudentId { get; set; }

    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public int Grade { get; set; }

    /// <summary>The owning identity. Required: every student profile has an owner.</summary>
    public User User { get; set; } = null!;
}
