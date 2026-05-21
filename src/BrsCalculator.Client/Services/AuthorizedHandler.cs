using System.Net.Http.Headers;

namespace BrsCalculator.Client.Services;

public class AuthorizedHandler : DelegatingHandler
{
    private readonly LocalStorageService _storage;

    public AuthorizedHandler(LocalStorageService storage) => _storage = storage;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _storage.GetAsync("brs_auth_token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
