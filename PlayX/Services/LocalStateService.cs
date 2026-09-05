using Microsoft.JSInterop;
using System.Text.Json;

namespace PlayX.Services;

public class LocalStateService
{
    private readonly IJSRuntime _js;

    public LocalStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SaveStateAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task<T?> GetStateAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", key);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task ClearKeyAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}