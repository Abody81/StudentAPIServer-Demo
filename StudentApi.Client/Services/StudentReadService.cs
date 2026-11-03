using StudentApi.Client.Infrastructure;
using StudentApi.Shared;
using StudentApi.Client.Services.Interfaces;

namespace StudentApi.Client.Services;

public class StudentReadService : IStudentReadService
{
    private readonly IApiClient _apiClient;

    public StudentReadService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<StudentDto>?> GetAllAsync()
    {
        var response = await _apiClient.GetAsync("api/Students/All");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<StudentDto>>()
            : null;
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var response = await _apiClient.GetAsync($"api/Students/{id}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StudentDto>()
            : null;
    }

    public async Task<List<StudentDto>?> GetPassedAsync()
    {
        var response = await _apiClient.GetAsync("api/Students/Passed");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<StudentDto>>()
            : null;
    }

    public async Task<double?> GetAverageGradeAsync()
    {
        var response = await _apiClient.GetAsync("api/Students/AverageGrade");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<double>()
            : null;
    }
}
