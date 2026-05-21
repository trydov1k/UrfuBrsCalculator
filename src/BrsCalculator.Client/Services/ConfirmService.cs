using Microsoft.JSInterop;

namespace BrsCalculator.Client.Services;

public class ConfirmService
{
    private readonly IJSRuntime _js;

    public ConfirmService(IJSRuntime js) => _js = js;

    public Task<bool> AskAsync(string message) =>
        _js.InvokeAsync<bool>("confirm", message).AsTask();
}
