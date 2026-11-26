namespace StudentBackend.Entities;

public class Student
{
    /// <summary>
    /// StudentId IS the account's identifier - one person, one id everywhere.
    /// The domain does not know who manages that identifier (that is an
    /// infrastructure concern) - it only owns its own value.
    /// </summary>
    public Guid StudentId { get; set; }

    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public int Grade { get; set; }
}
