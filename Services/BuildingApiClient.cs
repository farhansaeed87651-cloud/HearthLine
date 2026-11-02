using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Hearthline.Services;

public sealed class BuildingApiClient
{
    private readonly HttpClient _httpClient = new();

    public string? BaseUrl { get; } = Environment.GetEnvironmentVariable("HEARTHLINE_API_BASE_URL");
    public bool IsConfigured => Uri.TryCreate(BaseUrl, UriKind.Absolute, out var address) && address.Scheme is "http" or "https";

    public BuildingApiClient()
    {
        if (Uri.TryCreate(BaseUrl, UriKind.Absolute, out var address))
        {
            _httpClient.BaseAddress = address;
        }
    }

    public void SetAccessToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAccessToken() => SetAccessToken(null);

    public Task<T> GetJsonAsync<T>(string path, CancellationToken cancellationToken = default) => SendJsonAsync<T>(HttpMethod.Get, path, null, cancellationToken);

    public Task PostAsync(string path, object payload, CancellationToken cancellationToken = default) => SendAsync(HttpMethod.Post, path, payload, cancellationToken);

    public async Task<JsonDocument> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        using var content = new StringContent(JsonSerializer.Serialize(new { email, password }), Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("api/auth/login", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    public async Task CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        using var response = await _httpClient.GetAsync("health", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string path, object? payload, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        using var response = await SendRequestAsync(method, path, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken)
            ?? throw new InvalidOperationException("The building service returned an empty response.");
    }

    private async Task SendAsync(HttpMethod method, string path, object payload, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        using var response = await SendRequestAsync(method, path, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string path, object? payload, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, path);
        if (payload is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return _httpClient.SendAsync(request, cancellationToken);
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Set HEARTHLINE_API_BASE_URL to connect to a building service.");
    }
}