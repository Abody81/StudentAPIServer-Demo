using StudentApi.Client.Infrastructure;
using StudentApi.Shared;
using StudentApi.Shared.Requests;
using System.Net.Http.Json;

using StudentApi.Client.Services.Interfaces;

namespace StudentApi.Client.Services;

public class StudentWriteService : IStudentWriteService
{
    private readonly IApiClient _apiClient;

    public StudentWriteService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<StudentDto?> AddAsync(CreateStudentRequest request)
    {
        var response = await _apiClient.PostAsJsonAsync("api/Students", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StudentDto>()
            : null;
    }

    public async Task<StudentDto?> UpdateAsync(int id, StudentDto student)
    {
        var response = await _apiClient.PutAsJsonAsync($"api/Students/{id}", student);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StudentDto>()
            : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"api/Students/{id}");
        return response.IsSuccessStatusCode;
    }
}
