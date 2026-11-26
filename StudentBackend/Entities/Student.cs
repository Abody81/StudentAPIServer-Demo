namespace StudentBackend.Entities;

public class Student
{
    /// <summary>
    /// StudentId IS the account's id - one person, one id everywhere.
    /// The domain does not know who manages that id (that is an
    /// infrastructure concern) - it only owns its own number.
    /// </summary>
    public int StudentId { get; set; }

    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public int Grade { get; set; }
}
