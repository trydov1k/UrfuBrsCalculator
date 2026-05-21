using System.Net.Http.Json;
using BrsCalculator.Application.DTOs;

namespace BrsCalculator.Client.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public async Task<string?> GetErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body.Trim('"');
        }
        catch
        {
            return response.ReasonPhrase;
        }
    }

    public async Task<IReadOnlyList<SemesterDto>?> GetSemestersAsync() =>
        await _http.GetFromJsonAsync<List<SemesterDto>>("api/semesters");

    public async Task<SemesterDto?> CreateSemesterAsync(CreateSemesterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/semesters", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SemesterDto>()
            : null;
    }

    public async Task<bool> UpdateSemesterAsync(Guid id, UpdateSemesterRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/semesters/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSemesterAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/semesters/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<IReadOnlyList<SubjectListItemDto>?> GetSubjectsAsync(Guid semesterId) =>
        await _http.GetFromJsonAsync<List<SubjectListItemDto>>($"api/semesters/{semesterId}/subjects");

    public async Task<SubjectDetailDto?> GetSubjectAsync(Guid semesterId, Guid subjectId) =>
        await _http.GetFromJsonAsync<SubjectDetailDto>($"api/semesters/{semesterId}/subjects/{subjectId}");

    public async Task<(SubjectDetailDto? Result, string? Error)> CreateSubjectAsync(
        Guid semesterId, CreateSubjectRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/semesters/{semesterId}/subjects", request);
        if (!response.IsSuccessStatusCode)
            return (null, await GetErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<SubjectDetailDto>(), null);
    }

    public async Task<bool> UpdateSubjectAsync(Guid semesterId, Guid subjectId, UpdateSubjectRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/semesters/{semesterId}/subjects/{subjectId}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSubjectAsync(Guid semesterId, Guid subjectId)
    {
        var response = await _http.DeleteAsync($"api/semesters/{semesterId}/subjects/{subjectId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(SubjectDetailDto? Result, string? Error)> CreateNodeAsync(
        Guid semesterId, Guid subjectId, CreateNodeRequest request)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/semesters/{semesterId}/subjects/{subjectId}/nodes", request);
        if (!response.IsSuccessStatusCode)
            return (null, await GetErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<SubjectDetailDto>(), null);
    }

    public async Task<(SubjectDetailDto? Result, string? Error)> UpdateNodeAsync(
        Guid semesterId, Guid subjectId, Guid nodeId, UpdateNodeRequest request)
    {
        var response = await _http.PutAsJsonAsync(
            $"api/semesters/{semesterId}/subjects/{subjectId}/nodes/{nodeId}", request);
        if (!response.IsSuccessStatusCode)
            return (null, await GetErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<SubjectDetailDto>(), null);
    }

    public async Task<(SubjectDetailDto? Result, string? Error)> DeleteNodeAsync(
        Guid semesterId, Guid subjectId, Guid nodeId)
    {
        var response = await _http.DeleteAsync(
            $"api/semesters/{semesterId}/subjects/{subjectId}/nodes/{nodeId}");
        if (!response.IsSuccessStatusCode)
            return (null, await GetErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<SubjectDetailDto>(), null);
    }

    public async Task<SubjectDetailDto?> UpdateScoreAsync(
        Guid semesterId, Guid subjectId, Guid nodeId, decimal? score)
    {
        var response = await _http.PutAsJsonAsync(
            $"api/semesters/{semesterId}/subjects/{subjectId}/nodes/{nodeId}/score",
            new UpdateScoreRequest(score));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SubjectDetailDto>()
            : null;
    }

    public async Task<SubjectDetailDto?> PreviewAsync(
        Guid semesterId, Guid subjectId, IReadOnlyDictionary<Guid, decimal?> overrides) =>
        await PreviewRequestAsync(semesterId, subjectId, new WhatIfRequest(overrides, "4"));

    public async Task<WhatIfResultDto?> WhatIfAsync(
        Guid semesterId, Guid subjectId, WhatIfRequest request)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/semesters/{semesterId}/subjects/{subjectId}/what-if", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WhatIfResultDto>()
            : null;
    }

    private async Task<SubjectDetailDto?> PreviewRequestAsync(
        Guid semesterId, Guid subjectId, WhatIfRequest request)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/semesters/{semesterId}/subjects/{subjectId}/preview", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SubjectDetailDto>()
            : null;
    }
}
