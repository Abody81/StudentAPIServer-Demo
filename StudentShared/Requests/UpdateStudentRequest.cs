namespace StudentShared.Requests;

public class UpdateStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int Grade { get; set; }
}
