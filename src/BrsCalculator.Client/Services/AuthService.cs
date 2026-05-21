using System.Net.Http.Json;
using BrsCalculator.Application.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace BrsCalculator.Client.Services;

public class AuthService
{
    private const string TokenKey = "brs_auth_token";
    private readonly HttpClient _http;
    private readonly LocalStorageService _storage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, LocalStorageService storage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _storage = storage;
        _authStateProvider = authStateProvider;
    }

    public async Task<(AuthResponse? Auth, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        if (!response.IsSuccessStatusCode)
            return (null, await ReadErrorMessageAsync(response));

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is not null)
            await SetSessionAsync(auth);
        return (auth, null);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var errors = await response.Content.ReadFromJsonAsync<string[]>();
            if (errors is { Length: > 0 })
                return string.Join(" ", errors);
        }
        catch
        {
            // ignore parse errors
        }

        var raw = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(raw)
            ? "Не удалось выполнить запрос"
            : raw.Trim('"');
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is not null) await SetSessionAsync(auth);
        return auth;
    }

    public async Task<(bool Ok, string? ResetToken, string? Error)> ForgotPasswordAsync(string email)
    {
        var response = await _http.PostAsJsonAsync("api/auth/forgot-password", new ForgotPasswordRequest(email));
        if (!response.IsSuccessStatusCode)
            return (false, null, "Запрос не выполнен");
        try
        {
            var doc = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            return (true, doc?.ResetToken, null);
        }
        catch
        {
            return (true, null, null);
        }
    }

    public async Task<(bool Ok, string? Error)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/reset-password", request);
        if (response.IsSuccessStatusCode) return (true, null);
        var err = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(err) ? "Ошибка сброса пароля" : err.Trim('"'));
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveAsync(TokenKey);
        await _storage.RemoveAsync("brs_user_email");
        ((JwtAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
    }

    public async Task<string?> GetTokenAsync() => await _storage.GetAsync(TokenKey);

    private async Task SetSessionAsync(AuthResponse auth)
    {
        await _storage.SetAsync(TokenKey, auth.Token);
        await _storage.SetAsync("brs_user_email", auth.Email);
        ((JwtAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
    }

    private sealed record ForgotPasswordResponse(string? Message, string? ResetToken);
}
