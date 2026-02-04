using System.Net.Http.Json;
using UnifiedOnboarding.Registration.Features.OTP.Abstractions;

namespace UnifiedOnboarding.Registration.Features.OTP.Services;

public sealed class OtpTokenApi : IOtpTokenApi
{
    private readonly HttpClient _http;
    public OtpTokenApi(HttpClient http) => _http = http;

    public async Task<GetTokenResponseDto> GetOtpTokenAsync(GetTokenRequestDto request, CancellationToken cancellationToken)
    {
        // Build form-urlencoded body
        var form = new Dictionary<string, string?>
        {
            ["grant_type"] = request.GrantType,
            ["username"] = request.Username,
            ["password"] = request.Password,
            ["client_id"] = request.ClientId,
            ["client_secret"] = request.ClientSecret
        };
        using var content = new FormUrlEncodedContent(form!);
        // POST using form-urlencoded
        HttpResponseMessage response = await _http.PostAsync("connect/token", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        // Deserialize JSON response
        return await response.Content.ReadFromJsonAsync<GetTokenResponseDto>(cancellationToken: cancellationToken)
               ?? new GetTokenResponseDto(string.Empty, string.Empty, 0);
    }
}
