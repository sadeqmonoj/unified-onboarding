using System.Text.Json.Serialization;

namespace UnifiedOnboarding.Registration.Features.OTP.Abstractions;

public interface IOtpTokenApi
{
    Task<GetTokenResponseDto> GetOtpTokenAsync(GetTokenRequestDto request, CancellationToken cancellationToken);
}

public sealed record GetTokenRequestDto(
     [property: JsonPropertyName("grant_type")] string GrantType,
     [property: JsonPropertyName("username")] string Username,
     [property: JsonPropertyName("password")] string Password,
     [property: JsonPropertyName("client_id")] string ClientId,
     [property: JsonPropertyName("client_secret")] string ClientSecret);

public sealed record GetTokenResponseDto(string access_token, string token_type, decimal expires_in);
