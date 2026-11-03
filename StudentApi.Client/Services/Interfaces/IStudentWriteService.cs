using StudentApi.Shared;
using StudentApi.Shared.Requests;

namespace StudentApi.Client.Services.Interfaces;

public interface IStudentWriteService
{
    Task<StudentDto?> AddAsync(CreateStudentRequest request);
    Task<StudentDto?> UpdateAsync(int id, StudentDto student);
    Task<bool> DeleteAsync(int id);
}
