using Microsoft.JSInterop;

namespace BrsCalculator.Client.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js) => _js = js;

    public async Task SetAsync(string key, string value) =>
        await _js.InvokeVoidAsync("localStorage.setItem", key, value);

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    public async Task RemoveAsync(string key) =>
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
}
