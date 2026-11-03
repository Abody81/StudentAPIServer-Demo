using StudentApi.Shared;

namespace StudentApi.Client.Services.Interfaces;

public interface IStudentReadService
{
    Task<List<StudentDto>?> GetAllAsync();
    Task<StudentDto?> GetByIdAsync(int id);
    Task<List<StudentDto>?> GetPassedAsync();
    Task<double?> GetAverageGradeAsync();
}
